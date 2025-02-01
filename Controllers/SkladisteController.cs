using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SkladisteRobe.Data;
using SkladisteRobe.Models;
using SkladisteRobe.Services;
using System;
using System.Linq;
using System.Security.Claims;

namespace SkladisteRobe.Controllers
{
    [Authorize]
    public class SkladisteController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PdfService _pdfService;

        public SkladisteController(AppDbContext context)
        {
            _context = context;
            _pdfService = new PdfService();
        }

        // GET: /Skladiste/Index – Material overview
        public IActionResult Index(string searchString)
        {
            var materijali = _context.Materijali.AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                materijali = materijali.Where(m => m.Naziv.Contains(searchString));
            }
            return View(materijali.ToList());
        }

        // GET: /Skladiste/RadniNalog – Display the bulk transaction (Radni nalog) form.
        public IActionResult RadniNalog()
        {
            return View();
        }

        // POST: /Skladiste/RadniNalog – Process the bulk transaction form submission.
        // The submitType parameter must be either "Primka" (receipt) or "Izdaj robu" (issuance).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RadniNalog(BulkTransactionViewModel model, string submitType)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Check for duplicate names in the input list (case-insensitive).
            var duplicateNames = model.Items.GroupBy(x => x.Naziv.ToLower())
                                              .Where(g => g.Count() > 1)
                                              .Select(g => g.Key)
                                              .ToList();
            if (duplicateNames.Any())
            {
                ModelState.AddModelError("", "Ne možete unijeti više stavki s istim nazivom: " + string.Join(", ", duplicateNames));
                return View(model);
            }

            // Retrieve the logged-in user's ID and full name.
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            int userId = 0;
            if (userIdClaim != null)
                int.TryParse(userIdClaim.Value, out userId);
            var fullName = User.FindFirst("FullName")?.Value ?? "Nepoznati korisnik";

            // Process each bulk transaction item.
            foreach (var item in model.Items)
            {
                if (submitType == "Primka")
                {
                    // For receipt ("Primka"): if a material with the same name and unit exists, add quantity; otherwise, create a new material.
                    var existingMat = _context.Materijali
                        .FirstOrDefault(m => m.Naziv.ToLower() == item.Naziv.ToLower() && m.Jedinica == item.Jedinica);
                    if (existingMat != null)
                    {
                        existingMat.Kolicina += item.Kolicina;
                        _context.Transakcije.Add(new Transakcija
                        {
                            MaterijalId = existingMat.Id,
                            Kolicina = item.Kolicina,
                            Datum = DateTime.Now,
                            Tip = "Primka",
                            KorisnikId = userId
                        });
                    }
                    else
                    {
                        var newMat = new Materijal
                        {
                            Naziv = item.Naziv,
                            Kolicina = item.Kolicina,
                            Jedinica = item.Jedinica
                        };
                        _context.Materijali.Add(newMat);
                        _context.SaveChanges(); // Ensure newMat.Id is generated.
                        _context.Transakcije.Add(new Transakcija
                        {
                            MaterijalId = newMat.Id,
                            Kolicina = item.Kolicina,
                            Datum = DateTime.Now,
                            Tip = "Primka",
                            KorisnikId = userId
                        });
                    }
                }
                else if (submitType == "Izdaj robu")
                {
                    // For issuance ("Izdaj robu"): material must exist (with the same unit) and have sufficient quantity.
                    var existingMat = _context.Materijali
                        .FirstOrDefault(m => m.Naziv.ToLower() == item.Naziv.ToLower() && m.Jedinica == item.Jedinica);
                    if (existingMat == null)
                    {
                        ModelState.AddModelError("", $"Materijal s nazivom {item.Naziv} i jedinicom {item.Jedinica} ne postoji.");
                        return View(model);
                    }
                    if (existingMat.Kolicina < item.Kolicina)
                    {
                        ModelState.AddModelError("", $"Nema dovoljno količine za {item.Naziv} (jedinica: {item.Jedinica}). Trenutno ima: {existingMat.Kolicina}");
                        return View(model);
                    }
                    existingMat.Kolicina -= item.Kolicina;
                    _context.Transakcije.Add(new Transakcija
                    {
                        MaterijalId = existingMat.Id,
                        Kolicina = item.Kolicina,
                        Datum = DateTime.Now,
                        Tip = "Izdaj robu",
                        KorisnikId = userId
                    });
                }
                else
                {
                    ModelState.AddModelError("", "Nepoznat tip transakcije.");
                    return View(model);
                }
            }

            _context.SaveChanges();

            // Generate a bulk transaction (Radni nalog) PDF report.
            var pdfBytes = _pdfService.GenerateBulkTransactionPdf(model, submitType, fullName);
            string fileName = submitType == "Primka" ? "Primka.pdf" : "IzdajRobu.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        // GET: /Skladiste/Transakcije – Display a list of past transactions.
        public IActionResult Transakcije()
        {
            // Include the related Korisnik entity to display full name.
            var transakcije = _context.Transakcije
                .Include(t => t.Korisnik)
                .OrderByDescending(t => t.Datum)
                .ToList();
            return View(transakcije);
        }

        // GET: /Skladiste/GenerateTransakcijePdf – Generate a detailed PDF report for all past transactions.
        public IActionResult GenerateTransakcijePdf()
        {
            var transakcije = _context.Transakcije
                .Include(t => t.Korisnik)
                .OrderByDescending(t => t.Datum)
                .ToList();
            var pdfBytes = _pdfService.GenerateTransakcijePdf(transakcije);
            return File(pdfBytes, "application/pdf", "Transakcije.pdf");
        }

        // GET: /Skladiste/GeneratePdf/{id} – Generate a PDF for a specific transaction.
        public IActionResult GeneratePdf(int id)
        {
            var transakcija = _context.Transakcije
                .Include(t => t.Korisnik)
                .FirstOrDefault(t => t.Id == id);
            if (transakcija == null)
                return NotFound();
            var materijal = _context.Materijali.FirstOrDefault(m => m.Id == transakcija.MaterijalId);
            var pdfBytes = _pdfService.GeneratePdfReport(transakcija, materijal);
            return File(pdfBytes, "application/pdf", $"Transakcija_{transakcija.Id}.pdf");
        }

        // GET: /Skladiste/GenerateAllPdf – Generate a PDF report for all materials.
        public IActionResult GenerateAllPdf()
        {
            var materijali = _context.Materijali.ToList();
            var pdfBytes = _pdfService.GenerateAllMaterialsPdf(materijali);
            return File(pdfBytes, "application/pdf", "SviMaterijali.pdf");
        }
    }
}
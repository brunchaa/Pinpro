using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using SkladisteRobe.Models;

namespace SkladisteRobe.Services
{
    public class PdfService
    {//generira za jednu transakciju
        public byte[] GeneratePdfReport(Transakcija transakcija, Materijal materijal)
        {
            if (transakcija == null)
                throw new ArgumentNullException(nameof(transakcija), "Transaction cannot be null.");
            if (materijal == null)
                throw new ArgumentNullException(nameof(materijal), "Material cannot be null.");

            using (var ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                string naslov = transakcija.Tip == "Ulaz"
                    ? "Radni nalog za unos robe"
                    : "Radni nalog za otpremu robe";
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                doc.Add(new Paragraph(naslov, titleFont));
                doc.Add(new Paragraph(" ")); // Blank line
                doc.Add(new Paragraph($"Naziv materijala: {materijal.Naziv}"));
                doc.Add(new Paragraph($"Količina operacije: {transakcija.Kolicina}"));
                doc.Add(new Paragraph($"Tip operacije: {transakcija.Tip}"));
                doc.Add(new Paragraph($"Datum i vrijeme: {transakcija.Datum:dd.MM.yyyy HH:mm:ss}"));
                doc.Close();
                return ms.ToArray();
            }
        }
        // Generates a PDF report for all materials.
        public byte[] GenerateAllMaterialsPdf(List<Materijal> materijali)
        {
            using (var ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4.Rotate(), 25, 25, 30, 30);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                Paragraph title = new Paragraph("Pregled Materijala", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER
                };
                doc.Add(title);
                doc.Add(new Paragraph(" "));

                PdfPTable table = new PdfPTable(3) { WidthPercentage = 100 };
                table.AddCell(new PdfPCell(new Phrase("ID")) { BackgroundColor = BaseColor.LIGHT_GRAY });
                table.AddCell(new PdfPCell(new Phrase("Naziv")) { BackgroundColor = BaseColor.LIGHT_GRAY });
                table.AddCell(new PdfPCell(new Phrase("Količina")) { BackgroundColor = BaseColor.LIGHT_GRAY });
                foreach (var m in materijali)
                {
                    table.AddCell(m.Id.ToString());
                    table.AddCell(m.Naziv);
                    table.AddCell(m.Kolicina.ToString());
                }
                doc.Add(table);
                doc.Close();
                return ms.ToArray();
            }
        }

        // Generates a detailed PDF report for all past transactions.
        public byte[] GenerateTransakcijePdf(List<Transakcija> transakcije)
        {
            using (var ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                Paragraph title = new Paragraph("Pregled Prošlih Transakcija", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER
                };
                doc.Add(title);
                doc.Add(new Paragraph(" "));

                PdfPTable table = new PdfPTable(4) { WidthPercentage = 100 };
                table.AddCell(new PdfPCell(new Phrase("ID")) { BackgroundColor = BaseColor.LIGHT_GRAY });
                table.AddCell(new PdfPCell(new Phrase("Datum")) { BackgroundColor = BaseColor.LIGHT_GRAY });
                table.AddCell(new PdfPCell(new Phrase("Vrijeme")) { BackgroundColor = BaseColor.LIGHT_GRAY });
                table.AddCell(new PdfPCell(new Phrase("Kreirao")) { BackgroundColor = BaseColor.LIGHT_GRAY });

                foreach (var t in transakcije)
                {
                    table.AddCell(t.Id.ToString());
                    table.AddCell(t.Datum.ToString("dd.MM.yyyy"));
                    table.AddCell(t.Datum.ToString("HH:mm:ss"));
                    string creator = t.Korisnik != null ? $"{t.Korisnik.Ime} {t.Korisnik.Prezime}" : "Nepoznato";
                    table.AddCell(creator);
                }
                doc.Add(table);
                doc.Close();
                return ms.ToArray();
            }
        }

        // Generates a bulk transaction (Radni nalog) PDF report.
        public byte[] GenerateBulkTransactionPdf(BulkTransactionViewModel model, string transactionType, string employeeName)
        {
            using (var ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4.Rotate(), 25, 25, 30, 30);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                string naslov = transactionType == "Primka" ? "Radni nalog za unos robe" : "Radni nalog za otpremu robe";
                Paragraph title = new Paragraph(naslov, titleFont)
                {
                    Alignment = Element.ALIGN_CENTER
                };
                doc.Add(title);
                doc.Add(new Paragraph(" "));
                doc.Add(new Paragraph($"Kreirao: {employeeName}"));
                doc.Add(new Paragraph($"Datum: {DateTime.Now:dd.MM.yyyy HH:mm:ss}"));
                doc.Add(new Paragraph(" "));

                PdfPTable table = new PdfPTable(2) { WidthPercentage = 100 };
                table.AddCell(new PdfPCell(new Phrase("Naziv materijala")) { BackgroundColor = BaseColor.LIGHT_GRAY });
                table.AddCell(new PdfPCell(new Phrase("Količina")) { BackgroundColor = BaseColor.LIGHT_GRAY });

                foreach (var item in model.Items)
                {
                    table.AddCell(item.Naziv);
                    table.AddCell(item.Kolicina.ToString());
                }
                doc.Add(table);
                doc.Close();
                return ms.ToArray();
            }
        }
    }
}
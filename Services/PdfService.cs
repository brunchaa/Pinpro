using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using SkladisteRobe.Models;

namespace SkladisteRobe.Services
{
    public class PdfService
    {
        // Path to your Unicode font file that supports Croatian characters.
        // If the file extension is .ttf instead of .ttff, update the constant accordingly.
        private const string UnicodeFontPath = @"C:\Font\l_10646.ttf";

        // Generates a PDF report for a single transaction.
        public byte[] GeneratePdfReport(Transakcija transakcija, Materijal materijal)
        {
            if (transakcija == null)
                throw new ArgumentNullException(nameof(transakcija));
            if (materijal == null)
                throw new ArgumentNullException(nameof(materijal));

            // Create a BaseFont with Identity-H encoding to support Unicode.
            BaseFont baseFont = BaseFont.CreateFont(UnicodeFontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            Font titleFont = new Font(baseFont, 18, Font.BOLD);
            Font normalFont = new Font(baseFont, 12, Font.NORMAL);

            using (var ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                string naslov = transakcija.Tip == "Ulaz" ? "Radni nalog za unos robe" : "Radni nalog za otpremu robe";
                doc.Add(new Paragraph(naslov, titleFont));
                doc.Add(new Paragraph(" ", normalFont)); // Blank line
                doc.Add(new Paragraph($"Naziv materijala: {materijal.Naziv}", normalFont));
                // Using a Unicode escape for č if needed:
                doc.Add(new Paragraph($"Koli\u010Dina operacije: {transakcija.Kolicina}", normalFont));
                doc.Add(new Paragraph($"Tip operacije: {transakcija.Tip}", normalFont));
                doc.Add(new Paragraph($"Datum i vrijeme: {transakcija.Datum:dd.MM.yyyy HH:mm:ss}", normalFont));
                doc.Close();
                return ms.ToArray();
            }
        }

        // Generates a PDF report for all materials.
        public byte[] GenerateAllMaterialsPdf(List<Materijal> materijali)
        {
            BaseFont baseFont = BaseFont.CreateFont(UnicodeFontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            Font titleFont = new Font(baseFont, 18, Font.BOLD);
            Font normalFont = new Font(baseFont, 12, Font.NORMAL);

            using (var ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4.Rotate(), 25, 25, 30, 30);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                Paragraph title = new Paragraph("Pregled Materijala", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER
                };
                doc.Add(title);
                doc.Add(new Paragraph(" ", normalFont));

                PdfPTable table = new PdfPTable(3) { WidthPercentage = 100 };
                table.AddCell(new PdfPCell(new Phrase("ID", normalFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                table.AddCell(new PdfPCell(new Phrase("Naziv", normalFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                table.AddCell(new PdfPCell(new Phrase("Koli\u010Dina", normalFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                foreach (var m in materijali)
                {
                    table.AddCell(new Phrase(m.Id.ToString(), normalFont));
                    table.AddCell(new Phrase(m.Naziv, normalFont));
                    table.AddCell(new Phrase(m.Kolicina.ToString(), normalFont));
                }
                doc.Add(table);
                doc.Close();
                return ms.ToArray();
            }
        }

        // Generates a detailed PDF report for all past transactions.
        public byte[] GenerateTransakcijePdf(List<Transakcija> transakcije)
        {
            BaseFont baseFont = BaseFont.CreateFont(UnicodeFontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            Font titleFont = new Font(baseFont, 18, Font.BOLD);
            Font normalFont = new Font(baseFont, 12, Font.NORMAL);

            using (var ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                Paragraph title = new Paragraph("Pregled Prošlih Transakcija", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER
                };
                doc.Add(title);
                doc.Add(new Paragraph(" ", normalFont));

                PdfPTable table = new PdfPTable(4) { WidthPercentage = 100 };
                table.AddCell(new PdfPCell(new Phrase("ID", normalFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                table.AddCell(new PdfPCell(new Phrase("Datum", normalFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                table.AddCell(new PdfPCell(new Phrase("Vrijeme", normalFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                table.AddCell(new PdfPCell(new Phrase("Kreirao", normalFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                foreach (var t in transakcije)
                {
                    table.AddCell(new Phrase(t.Id.ToString(), normalFont));
                    table.AddCell(new Phrase(t.Datum.ToString("dd.MM.yyyy"), normalFont));
                    table.AddCell(new Phrase(t.Datum.ToString("HH:mm:ss"), normalFont));
                    string creator = t.Korisnik != null ? $"{t.Korisnik.Ime} {t.Korisnik.Prezime}" : "Nepoznato";
                    table.AddCell(new Phrase(creator, normalFont));
                }
                doc.Add(table);
                doc.Close();
                return ms.ToArray();
            }
        }

        // Generates a bulk transaction (Radni nalog) PDF report.
        public byte[] GenerateBulkTransactionPdf(BulkTransactionViewModel model, string transactionType, string employeeName)
        {
            BaseFont baseFont = BaseFont.CreateFont(UnicodeFontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            Font titleFont = new Font(baseFont, 18, Font.BOLD);
            Font normalFont = new Font(baseFont, 12, Font.NORMAL);

            using (var ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4.Rotate(), 25, 25, 30, 30);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                string naslov = transactionType == "Primka"
                    ? "Radni nalog za unos robe"
                    : "Radni nalog za otpremu robe";
                Paragraph title = new Paragraph(naslov, titleFont)
                {
                    Alignment = Element.ALIGN_CENTER
                };
                doc.Add(title);
                doc.Add(new Paragraph(" ", normalFont));
                doc.Add(new Paragraph($"Kreirao: {employeeName}", normalFont));
                doc.Add(new Paragraph($"Datum: {DateTime.Now:dd.MM.yyyy HH:mm:ss}", normalFont));
                doc.Add(new Paragraph(" ", normalFont));

                PdfPTable table = new PdfPTable(2) { WidthPercentage = 100 };
                table.AddCell(new PdfPCell(new Phrase("Naziv materijala", normalFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                table.AddCell(new PdfPCell(new Phrase("Koli\u010Dina", normalFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });

                foreach (var item in model.Items)
                {
                    table.AddCell(new Phrase(item.Naziv, normalFont));
                    table.AddCell(new Phrase(item.Kolicina.ToString(), normalFont));
                }
                doc.Add(table);
                doc.Close();
                return ms.ToArray();
            }
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Layout.Borders;

namespace Digitization.Controllers
{
    public class POController : Controller
    {
        [HttpGet("GeneratePO")]
        public IActionResult GeneratePO()
        {
            return View();
        }

        [HttpGet("GeneratePOAsync")]
        public async Task<IActionResult> GeneratePOAsync()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                PdfWriter writer = new PdfWriter(stream);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);

                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                PdfFont normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                // ✅ Fix: Remove Duplicate Header
                document.Add(new Paragraph("Purchase Order")
                    .SetFont(boldFont)
                    .SetFontSize(16)
                    .SetTextAlignment(TextAlignment.CENTER));

                document.Add(new Paragraph("\n"));

                // ✅ Fix: Correct Image Path
                string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string imagePath = Path.Combine(wwwRootPath, "images", "intelligent_logo.png");

                if (System.IO.File.Exists(imagePath))
                {
                    Image logo = new Image(ImageDataFactory.Create(imagePath));
                    logo.ScaleToFit(100, 50); // Adjust size

                    // ✅ Fix: Ensure the position is set before adding to document
                    logo.SetFixedPosition(pdf.GetDefaultPageSize().GetWidth() - 120, pdf.GetDefaultPageSize().GetHeight() - 60);
                    document.Add(logo);
                }

                document.SetMargins(10, 20, 20, 20); // (top, right, bottom, left)

                document.Add(new Paragraph("\n"));

                // Define Table with 2 Columns
                Table table = new Table(2); // Two columns
                table.SetWidth(UnitValue.CreatePercentValue(100)); // Full width of page

                // Create Left Section (Company Details) inside a Bordered Cell
                Cell leftCell = new Cell();
                leftCell.SetBorder(new SolidBorder(1)); // Border for left section
                leftCell.SetPadding(10); // Space inside the box

                leftCell.Add(new Paragraph("Intelligent Autocontrol Pvt. Ltd.").SetFont(boldFont).SetFontSize(12));

                leftCell.Add(new Paragraph("Plot No. 1510/46/31, Khasra no. 7727/271/1\n" +
                                           "Surat Nagar Phase-II, Dhanwapur Road Gurgaon\n" +
                                           "Haryana - 122006").SetFont(normalFont).SetFontSize(10));

                leftCell.Add(new Paragraph("Email: sales@icona.in").SetFont(normalFont).SetFontSize(10));

                leftCell.Add(new Paragraph("GST No.: 06AACCI1709H1ZW").SetFont(normalFont).SetFontSize(10));

                table.AddCell(leftCell); // Add Left Section to Table

                // Create Right Section (Other Data)
                Cell rightCell = new Cell();
                rightCell.SetPadding(10);
                rightCell.Add(new Paragraph("PO No. ").SetFont(normalFont).SetFontSize(12));

                rightCell.Add(new Paragraph("PO date ").SetFont(normalFont).SetFontSize(12));

                rightCell.Add(new Paragraph("Delivery Terms ").SetFont(normalFont).SetFontSize(12));

                rightCell.Add(new Paragraph("Packing & Forwarding: ").SetFont(normalFont).SetFontSize(12));

                rightCell.Add(new Paragraph("Freight ").SetFont(normalFont).SetFontSize(12));

                rightCell.Add(new Paragraph("Insurance ").SetFont(normalFont).SetFontSize(12));

                rightCell.Add(new Paragraph("Payment Terms ").SetFont(normalFont).SetFontSize(12));

                table.AddCell(rightCell); // Add Right Section to Table

                document.Add(table);
                document.Add(new Paragraph("\n"));

                // Vendor & Shipping Details Table
                Table vendorTable = new Table(2);
                vendorTable.SetWidth(UnitValue.CreatePercentValue(100));

                vendorTable.AddCell(GetCell("Vendor Details:", 12, true, boldFont));
                vendorTable.AddCell(GetCell("Shipping Details:", 12, true, boldFont));

                vendorTable.AddCell(GetCell("Vendor Company Name\nAddress Line 1\nAddress Line 2\nGST No.: 06AACCI1709H1ZW", 10, false, normalFont));
                vendorTable.AddCell(GetCell("---Our Company Details---\nAddress Line 1\nAddress Line 2\nGST No.: 06AACCI1709H1ZW", 10, false, normalFont));

                document.Add(vendorTable);
                document.Add(new Paragraph("\n"));

                // PO Table Header
                Table poTable = new Table(8);
                poTable.SetWidth(UnitValue.CreatePercentValue(100));

                string[] headers = { "S.No", "Item Code", "Description", "HSN", "Qty", "Unit", "Rate", "Amount" };
                foreach (var header in headers)
                {
                    poTable.AddCell(GetCell(header, 10, true, boldFont, ColorConstants.LIGHT_GRAY));
                }

                // Sample Data Rows
                string[,] data = {
                    {"1", "IPA0001", "3 Pole MCB C-Curve 6A", "85002100", "10", "Nos.", "2,550.00", "10,200.00"},
                    {"2", "IPA0002", "3 Pole MCB C-Curve 10A", "85002101", "5", "Nos.", "2,550.00", "5,100.00"}
                };

                for (int i = 0; i < data.GetLength(0); i++)
                {
                    for (int j = 0; j < data.GetLength(1); j++)
                    {
                        poTable.AddCell(GetCell(data[i, j], 10, false, normalFont));
                    }
                }

                document.Add(poTable);
                document.Add(new Paragraph("\n"));

                // Footer Details
                document.Add(new Paragraph("PO Value in Words: Forty Eight Thousand One Hundred Forty Four Only")
                    .SetFont(boldFont)); // ✅ Use SetFont() for bold text

                document.Add(new Paragraph("\nDetailed Terms & Conditions:\nTerms in detail Text will be given.")
                    .SetFontSize(10));

                document.Add(new Paragraph("\nSign:\nDate:\nStamp:"));

                document.Close();
                return File(stream.ToArray(), "application/pdf", "PurchaseOrder.pdf");
            }
        }

        // ✅ Helper Method to Create Table Cells with Font Styling
        private Cell GetCell(string text, int fontSize, bool isBold, PdfFont font, Color bgColor = null)
        {
            Paragraph paragraph = new Paragraph(text)
                .SetFont(font)  // ✅ Set font dynamically (bold/normal)
                .SetFontSize(fontSize);

            Cell cell = new Cell().Add(paragraph).SetPadding(5);

            if (bgColor != null)
            {
                cell.SetBackgroundColor(bgColor);
            }

            return cell;
        }
    }
}

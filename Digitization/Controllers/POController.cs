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
using iTextSharp.text.pdf;
using iTextSharp.text;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System.IO;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using Digitization.Services;

namespace Digitization.Controllers
{
    [Authorize(Policy = "SharedAccess")]
    public class POController : Controller
    {
        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _configuration;

        public POController(ApplicationDBContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        [HttpGet]
        public IActionResult GeneratePO()
        {
            return View();
        }

        [HttpPost]
        [Route("PO/GeneratePOAsync")]
        public async Task<IActionResult> GeneratePOAsync([FromBody] PORequest formData)
        {
            Console.WriteLine("GeneratePOAsync");
            using (MemoryStream stream = new MemoryStream())
            {
                Console.WriteLine("Received Data: " + JsonConvert.SerializeObject(formData));

                var partyData = _context.PartyMaster.FirstOrDefault(p => p.PartyName == formData.PartyName);

                iText.Kernel.Pdf.PdfWriter writer = new iText.Kernel.Pdf.PdfWriter(stream);
                iText.Kernel.Pdf.PdfDocument pdf = new iText.Kernel.Pdf.PdfDocument(writer);

                iText.Layout.Document document = new iText.Layout.Document(pdf);
                document.SetMargins(0, 2, 0, 2);

                iText.Kernel.Font.PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                iText.Kernel.Font.PdfFont normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                // Create a 3-column table (Left Space, Title, Logo)
                Table headerTable = new Table(UnitValue.CreatePercentArray(new float[] { 3f, 6f, 2f }));
                headerTable.SetWidth(UnitValue.CreatePercentValue(100));

                // Empty Left Cell (For Spacing)
                Cell emptyCell = new Cell()
                    .Add(new iText.Layout.Element.Paragraph("")) // Empty content
                    .SetBorder(Border.NO_BORDER);

                headerTable.AddCell(emptyCell);

                // Title Cell (Centered)
                Cell titleCell = new Cell()
                    .Add(new iText.Layout.Element.Paragraph("Purchase Order")
                    .SetFont(boldFont)
                    .SetFontSize(16)
                    .SetTextAlignment(TextAlignment.CENTER)) // Center horizontally
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE) // Center vertically
                    .SetBorder(Border.NO_BORDER).SetPadding(0) // Remove extra padding
                    .SetMarginBottom(-5); // Adjust spacing below the title;

                headerTable.AddCell(titleCell);

                // Logo Cell (Right Side)
                string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string imagePath = Path.Combine(wwwRootPath, "images", "intelligent_logo.png");

                iText.Layout.Element.Image logo = new iText.Layout.Element.Image(iText.IO.Image.ImageDataFactory.Create(imagePath));
                logo.ScaleToFit(150, 80);

                Cell logoCell = new Cell()
                    .Add(logo)
                    .SetTextAlignment(TextAlignment.RIGHT) // Align to the right
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE) // Align vertically
                    .SetBorder(Border.NO_BORDER);

                headerTable.SetMarginBottom(-20); // Adjust this value as needed

                headerTable.AddCell(logoCell);

                // Add the table to the document
                document.Add(headerTable);

                document.SetMargins(0, 2, 0, 2);
                document.Add(new iText.Layout.Element.Paragraph("\n"));

                // ✅ Company Details Table
                Table companyTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 1 }));
                companyTable.SetWidth(UnitValue.CreatePercentValue(100));

                Cell leftCell = new Cell()
                    .Add(new iText.Layout.Element.Paragraph("Intelligent Autocontrol Pvt. Ltd.")
                        .SetFont(boldFont)
                        .SetFontSize(12))
                    .Add(new iText.Layout.Element.Paragraph("Plot No. 1510/46/31, Khasra no. 7727/271/1\n" +
                                       "Surat Nagar Phase-II, Dhanwapur Road Gurgaon\n" +
                                       "Haryana - 122006")
                        .SetFont(boldFont)
                        .SetFontSize(9))
                    .Add(new iText.Layout.Element.Paragraph("Email: ").SetFont(boldFont).SetFontSize(9))
                    .Add(new iText.Layout.Element.Paragraph("\n").SetFont(boldFont).SetFontSize(9))
                    .Add(new iText.Layout.Element.Paragraph("GST No.: 06AACCI1709H1ZW").SetFont(boldFont).SetFontSize(9))
                    .SetBorder(new SolidBorder(1));

                leftCell.SetMarginTop(-30);
                companyTable.AddCell(leftCell);

                Cell rightCell = new Cell().SetBorder(new SolidBorder(1));
                Table poDetailsTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 1 }));

                string[] labels = { "PO No.", "PO Date", "Delivery Terms", "Packing & Forwarding", "Freight", "Insurance", "Payment Terms" };

                foreach (string label in labels)
                {
                    poDetailsTable.AddCell(new Cell()
                        .Add(new iText.Layout.Element.Paragraph(label).SetFont(boldFont).SetFontSize(9))
                        .SetBorder(Border.NO_BORDER)
                        .SetTextAlignment(TextAlignment.LEFT));

                    poDetailsTable.AddCell(new Cell()
                        .Add(new iText.Layout.Element.Paragraph(":").SetFont(boldFont).SetFontSize(9))
                        .SetBorder(Border.NO_BORDER)
                        .SetTextAlignment(TextAlignment.LEFT));
                }

                rightCell.SetMarginTop(-30);
                rightCell.SetTextAlignment(TextAlignment.LEFT);
                rightCell.Add(poDetailsTable);
                companyTable.AddCell(rightCell);

                document.Add(companyTable);
                document.Add(new iText.Layout.Element.Paragraph("\n"));

                // Create a two-column table
                Table vendorShippingTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 1 }));
                vendorShippingTable.SetWidth(UnitValue.CreatePercentValue(100));
                vendorShippingTable.SetBorder(new SolidBorder(1));

                // Vendor Details Column
                Cell vendorCell = new Cell()
                    .Add(new iText.Layout.Element.Paragraph($"Vendor Details: ").SetFont(normalFont).SetFontSize(12))
                    .Add(new iText.Layout.Element.Paragraph($"{formData.PartyName}").SetFont(boldFont).SetFontSize(11))
                    .Add(new iText.Layout.Element.Paragraph($"{formData.Address}").SetFont(boldFont).SetFontSize(9))
                    //.Add(new iText.Layout.Element.Paragraph("Address Line-2").SetFont(boldFont).SetFontSize(9))
                    .Add(new iText.Layout.Element.Paragraph($"GST No.: {partyData.GSTNo}").SetFont(boldFont).SetFontSize(9))
                    .Add(new iText.Layout.Element.Paragraph($"\nVendor Contact Person : {partyData.ContactPerson}").SetFont(boldFont).SetFontSize(9))
                    .Add(new iText.Layout.Element.Paragraph($"Mobile No.: {partyData.ContactNo} Vendor Email: ${partyData.Email}").SetFont(boldFont).SetFontSize(9))
                    .SetBorder(new SolidBorder(1)); // Border for separation

                //vendorCell.SetWidth(UnitValue.CreatePercentValue(49));
                vendorCell.SetMarginRight(10); // Adjust as needed
                vendorShippingTable.AddCell(vendorCell);

                // Shipping Details Column
                Cell shippingCell = new Cell()
                    .Add(new iText.Layout.Element.Paragraph("Shipping Details:").SetFont(normalFont).SetFontSize(12))
                    .Add(new iText.Layout.Element.Paragraph("Intelligent Autocontrol Pvt. Ltd.").SetFont(boldFont).SetFontSize(11))
                    .Add(new iText.Layout.Element.Paragraph("1510/46/31, Khasra no. 7727/271/1, Surat nagar").SetFont(boldFont).SetFontSize(9))
                    .Add(new iText.Layout.Element.Paragraph("Phase-II, (Daultabad Chungi) Dhanwapur road \n Gurgaon Haryana - 122006").SetFont(boldFont).SetFontSize(9))
                    .Add(new iText.Layout.Element.Paragraph("GST No.: 06AACCI1709H1ZW").SetFont(boldFont).SetFontSize(9))
                    .Add(new iText.Layout.Element.Paragraph($"\n Contact Person ${formData.SenderName}").SetFont(boldFont).SetFontSize(9))
                    .Add(new iText.Layout.Element.Paragraph("Mobile No.: ..............   Vendor Email: ........").SetFont(boldFont).SetFontSize(9))
                    .SetBorder(new SolidBorder(1));

                vendorShippingTable.SetMarginTop(-20);

                shippingCell.SetMarginLeft(10);

                vendorShippingTable.AddCell(shippingCell);

                // Add this table to the document
                document.Add(vendorShippingTable);

                // Define column widths for table
                float[] columnWidths = { 2, 4, 7, 2, 1, 1, 2, 4, 2, 4, 4 };

                Table itemTable = new Table(UnitValue.CreatePercentArray(columnWidths));
                itemTable.SetWidth(UnitValue.CreatePercentValue(100)); // Full width
                itemTable.SetMarginTop(10);

                // Add Header Row
                string[] headers = { "S No.", "Item Code", "Description of goods", "HSN", "Qty", "Unit", "Rate", "Discount %", "Amount", "GST Rate", "GST Amount" };
                foreach (var header in headers)
                {
                    itemTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph(header).SetFont(boldFont).SetFontSize(10)).SetTextAlignment(TextAlignment.CENTER));
                }

                var items = formData.TableData;
                // Populate the table with item data
                foreach (var row in items)
                {
                    //itemTable.AddCell(GetCell(row.ItemCode?.ToString() ?? "", 10, false, normalFont));
                    //itemTable.AddCell(GetCell(row.ItemDescription?.ToString() ?? "", 10, false, normalFont));
                    //itemTable.AddCell(GetCell(row.HsnSac?.ToString() ?? "", 10, false, normalFont));
                    //itemTable.AddCell(GetCell(row.UnitRate?.ToString() ?? "", 10, false, normalFont));
                    //itemTable.AddCell(GetCell(row.Quantity?.ToString() ?? "", 10, false, normalFont));
                    //itemTable.AddCell(GetCell(row.Discount?.ToString() ?? "", 10, false, normalFont));
                    //itemTable.AddCell(GetCell(row.Amount?.ToString() ?? "", 10, false, normalFont));
                    //itemTable.AddCell(GetCell(row.GstRate?.ToString() ?? "", 10, false, normalFont));
                    //itemTable.AddCell(GetCell(row.GstAmount?.ToString() ?? "", 10, false, normalFont));
                    itemTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph(row.ItemCode).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER)));
                    itemTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph(row.ItemDescription).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER)));
                    itemTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph(row.HsnSac).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER)));
                    itemTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph(row.UnitRate).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER)));
                    itemTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph(row.Quantity).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER)));
                    itemTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph(row.Amount).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER)));
                    itemTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph(row.GstRate).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER)));
                    itemTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph(row.GstAmount).SetFontSize(9).SetTextAlignment(TextAlignment.CENTER)));
                }

                // Add Total Row
                Cell totalCell = new Cell(1, 8)
                    .Add(new iText.Layout.Element.Paragraph("Total").SetFont(boldFont).SetFontSize(10))
                    .SetTextAlignment(TextAlignment.RIGHT);
                itemTable.AddCell(totalCell);

                Cell totalAmount = new Cell()
                    .Add(new iText.Layout.Element.Paragraph("40,800.00").SetFont(boldFont).SetFontSize(10))
                    .SetTextAlignment(TextAlignment.CENTER);
                itemTable.AddCell(totalAmount);

                Cell emptyCell2 = new Cell()
                    .Add(new iText.Layout.Element.Paragraph("").SetFont(normalFont))
                    .SetTextAlignment(TextAlignment.CENTER);
                itemTable.AddCell(emptyCell2);

                Cell totalGST = new Cell()
                    .Add(new iText.Layout.Element.Paragraph("7,344.00").SetFont(boldFont).SetFontSize(10))
                    .SetTextAlignment(TextAlignment.CENTER);
                itemTable.AddCell(totalGST);

                // Add table to the PDF document
                document.Add(itemTable);

                // Main table structure
                float[] mainTableWidths = { 7, 3 };
                Table mainTable = new Table(UnitValue.CreatePercentArray(mainTableWidths))
                    .SetWidth(UnitValue.CreatePercentValue(100));

                // PO Value in Words & Remarks Section
                float[] poValueWidths = { 3, 8 };
                Table poValueTable = new Table(UnitValue.CreatePercentArray(poValueWidths))
                    .SetWidth(UnitValue.CreatePercentValue(100))
                    .SetHeight(UnitValue.CreatePercentValue(40));

                // Row 1: PO Value in Words
                poValueTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph("PO Value in Words:").SetFont(boldFont))
                    .SetBorder(new SolidBorder(1)).SetFontSize(10));
                poValueTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph("Forty Eight Thousand One Hundred Forty Four only"))
                    .SetBorder(new SolidBorder(1)).SetFontSize(10));

                // Row 2: Remarks
                poValueTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph("Remarks:").SetFont(boldFont))
                    .SetBorder(new SolidBorder(1)));
                poValueTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph("")) // Empty remarks
                    .SetHeight(20)
                    .SetBorder(new SolidBorder(1)));

                mainTable.AddCell(new Cell().Add(poValueTable).SetBorder(Border.NO_BORDER));

                // Financial Breakdown Table
                float[] breakdownWidths = { 7, 3 };
                Table breakdownTable = new Table(UnitValue.CreatePercentArray(breakdownWidths));

                breakdownTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph("Amount")).SetFont(boldFont));
                breakdownTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph("48,144.00")).SetTextAlignment(TextAlignment.RIGHT));

                breakdownTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph("P&F")));
                breakdownTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph("-")).SetTextAlignment(TextAlignment.RIGHT));

                breakdownTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph("Insurance")));
                breakdownTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph("-")).SetTextAlignment(TextAlignment.RIGHT));

                breakdownTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph("Freight")));
                breakdownTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph("-")).SetTextAlignment(TextAlignment.RIGHT));

                // PO Total Row (Make it Bold)
                breakdownTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph("PO Total")).SetFont(boldFont));
                breakdownTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph("48,144.00")).SetFont(boldFont).SetTextAlignment(TextAlignment.RIGHT));

                mainTable.AddCell(new Cell().Add(breakdownTable).SetBorder(Border.NO_BORDER));

                // Add Main Table to Document
                document.Add(mainTable);

                // 2. Detailed Terms & Conditions
                Table termsTable = new Table(1).SetWidth(UnitValue.CreatePercentValue(100));

                termsTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph("Detailed terms & Conditions:").SetFont(boldFont)).SetBorder(Border.NO_BORDER));
                termsTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph("Terms in detail text will be given")).SetBorder(Border.NO_BORDER));

                mainTable.AddCell(new Cell().Add(termsTable).SetBorder(new SolidBorder(1)));

                // 4. Signature Box
                Table signTable = new Table(1).SetWidth(UnitValue.CreatePercentValue(100));

                signTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph("Sign").SetFont(boldFont)).SetTextAlignment(TextAlignment.CENTER).SetBorder(Border.NO_BORDER));
                signTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph("Date")).SetTextAlignment(TextAlignment.CENTER).SetBorder(Border.NO_BORDER));
                signTable.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph("Stamp")).SetTextAlignment(TextAlignment.CENTER).SetBorder(Border.NO_BORDER));

                // Add Signature Box
                mainTable.AddCell(new Cell().Add(signTable).SetBorder(Border.NO_BORDER));

                document.Add(new iText.Layout.Element.Paragraph("\nDetailed Terms & Conditions:\nTerms in detail Text will be given.")
                    .SetFontSize(10));

                document.Add(new iText.Layout.Element.Paragraph("\nSign:\nDate:\nStamp:"));

                document.Close();
                byte[] fileBytes = stream.ToArray();
                return File(fileBytes, "application/pdf", "PurchaseOrder.pdf");
            }
        }

        public class PORequest
        {
            public string? PartyName { get; set; }
            public string ContactPerson { get; set; }
            public string Address { get; set; }
            public string MobileNo { get; set; }
            public string SenderName { get; set; }
            public string Subject { get; set; }
            public List<TableData> TableData { get; set; }
        }

        public class TableData
        {
            public string ItemCode { get; set; }
            public string ItemDescription { get; set; }
            public string HsnSac { get; set; }
            public string UnitRate { get; set; }
            public string Quantity { get; set; }
            public string Discount { get; set; }
            public string Amount { get; set; }
            public string GstRate { get; set; }
            public string GstAmount { get; set; }
        }

        // ✅ Helper Method to Create Table Cells with Font Styling
        private iText.Layout.Element.Cell GetCell(string text, int fontSize, bool isBold, iText.Kernel.Font.PdfFont font, iText.Kernel.Colors.Color bgColor = null)
        {
            iText.Layout.Element.Paragraph paragraph = new iText.Layout.Element.Paragraph(text)
                .SetFont(font)
                .SetFontSize(fontSize);

            iText.Layout.Element.Cell cell = new iText.Layout.Element.Cell().Add(paragraph).SetPadding(5);

            if (bgColor != null)
            {
                cell.SetBackgroundColor(bgColor);
            }

            return cell;
        }

        private string GetFinancialYear(string currentDate)
        {
            DateTime date = DateTime.Parse(currentDate); // Parse the input date

            int year = date.Year;

            // Check if the month is before April (i.e., January to March)
            if (date.Month < 4)
            {
                year--; // If before April, use the previous year for the financial year
            }

            // Format the financial year as "yy_yy" (e.g., "24_25")
            return $"{year.ToString().Substring(2)}-{(year + 1).ToString().Substring(2)}";
        }
    }
}

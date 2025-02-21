using Digitization.Models;
using Digitization.Services;
using Digitization.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using static iTextSharp.text.pdf.AcroFields;

namespace Digitization.Controllers
{
    public class Challan : Controller
    {
        private readonly ApplicationDBContext _context;

        public Challan(ApplicationDBContext context)
        {
            _context = context;
        }

        public IActionResult PartyDetails()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetID()
        {
            try
            {
                var idRecord = await _context.PartyMaster.OrderByDescending(x => x.RecordID).FirstOrDefaultAsync();
                return Json(new { partid = idRecord?.PartyID ?? 0 }); // Return JSON response
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> SaveParty(PartyMaster party)
        {
            try
            {

                party.EntryDTime = DateTime.Now;
                party.UpdateDTime = DateTime.Now;

                _context.PartyMaster.Add(party);
                await _context.SaveChangesAsync();

                // Format PartyID based on its length
                string formattedPartyID = party.PartyID.ToString().PadLeft(4, '0'); // Pads the PartyID with leading zeros if needed
                string formattedID;

                switch (party.PartyStatus)
                {
                    case 1:
                        formattedID = "C" + formattedPartyID;
                        break;
                    case 2:
                        formattedID = "V" + formattedPartyID;
                        break;
                    case 3:
                        formattedID = "B" + formattedPartyID;
                        break;
                    default:
                        formattedID = "" + formattedPartyID;
                        break;
                }







                return Json(new { success = true, partyID = formattedID });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
                //  return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }





        public IActionResult GenerateChallan()
        {
            return View();
        }


        [HttpGet]
        [Route("search-party")]
        public async Task<IActionResult> SearchParty(string search)
        {
            // Perform LIKE search using EF Core
            var results = await _context.PartyMaster
                .Where(p => EF.Functions.Like(p.PartyName, $"%{search}%"))
                .Select(p => new
                {
                    PartyID = p.PartyID,
                    PartyName = p.PartyName
                })
                .Take(5) // Limit results to 5
                .ToListAsync();

            return Json(results);
        }



        [HttpPost]
        public IActionResult SaveData([FromBody] SaveChallanData request)
        {
            if (request.Items == null || !request.Items.Any())
            {
                return BadRequest("No data provided.");
            }
            var data = _context.GenratedChallan.OrderByDescending(x => x.RecordID).FirstOrDefault();

            string challanFY = GetFinancialYear(request.CurrentDate);
            string challanNo = "";
            string Challan_Type = request.Challan_Type;

            foreach (var item in request.Items)
            {
                var generatedChallan = new GenratedChallan
                {
                    Challan_ID = data.Challan_ID + 1, // For example, generate Challan_ID dynamically
                    Challan_FY = challanFY, // Set the dynamic financial year
                    DTime = DateTime.Parse(request.CurrentDate), // Set the current time
                    Challan_Type = int.Parse(request.Challan_Type), // Assuming PartyStatus reflects Challan_Type
                    PartyID = request.PartyID,
                    DescriptionOfGoods = item.Description,
                    Qty = int.Parse(item.Qty), // Convert to int
                    Remark = item.Remark
                };

                _context.GenratedChallan.Add(generatedChallan);
                challanNo = generatedChallan.Challan_ID.ToString(); // Assign Challan No.
            }


            _context.SaveChanges();


            // Create Challan No. based on PartyStatus
            string finalChallanNo = GenerateChallanNo(Challan_Type, challanFY, challanNo);

            // Return the generated Challan No.
            return Ok(new { challanNo = finalChallanNo, realchallanNo = challanNo });

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

        // Method to generate the Challan No. based on PartyStatus and Financial Year
        private string GenerateChallanNo(string partyStatus, string challanFY, string challanNo)
        {
            if (partyStatus == "1") // If PartyStatus is IN
            {
                return $"IAPL/IN/{challanFY}/{challanNo}";
            }
            else if (partyStatus == "2") // If PartyStatus is IN
            {
                return $"IAPL/OUT/{challanFY}/{challanNo}";
            }
            else// If PartyStatus is IN
            {
                return $"IAPL/{challanFY}/{challanNo}";
            }
        }








        // use for pdf Generate

        // Endpoint for downloading PDF
        [HttpGet]
        public IActionResult DownloadPDF(string challanNo)
        {
            // Generate PDF using the Challan No. (you can use a library like iTextSharp or any other PDF library)

            // Example response
            byte[] pdfData = GeneratePDF(challanNo); // Assume GeneratePDF() is your method for creating the PDF

            // Return PDF file for download
            return File(pdfData, "application/pdf", "Challan_" + challanNo + ".pdf");
        }
        private byte[] GeneratePDF(string challanNo)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4);
                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);

                // Add the Footer to the PdfWriter
                writer.PageEvent = new Footer();  // This will add the footer on every page

                document.Open();


                // Create a table with two columns (Logo + Company Details)
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 1f, 3f }); // Adjust column width ratio

                // Left side: Company Logo
                string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/intelligent_logo.png");
                if (System.IO.File.Exists(logoPath))
                {
                    Image logo = Image.GetInstance(logoPath);
                    logo.ScaleToFit(185, 185); // Resize the logo
                    PdfPCell logoCell = new PdfPCell(logo)
                    {
                        Border = Rectangle.NO_BORDER,
                        Padding = 0,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    headerTable.AddCell(logoCell);
                }
                else
                {
                    headerTable.AddCell(new PdfPCell(new Phrase("")) { Border = Rectangle.NO_BORDER });
                }

                // Right side: Company Details (Multi-line)
                Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.BLACK); // Company Name
                Font addressFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK); // Address
                Font gstFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.BLACK); // GST No.

                // Create a right-aligned company details block
                Paragraph companyDetails = new Paragraph();
                companyDetails.Alignment = Element.ALIGN_RIGHT; // ✅ Align text to right

                companyDetails.Add(new Paragraph("Intelligent Autocontrol Pvt. Ltd.", titleFont)
                { SpacingAfter = 0, Alignment = Element.ALIGN_RIGHT });

                companyDetails.Add(new Paragraph("1510/46/31, Khasra no. 7727/271/1, Surat nagar", addressFont)
                { SpacingAfter = 0, Alignment = Element.ALIGN_RIGHT });

                companyDetails.Add(new Paragraph("Phase-II, (Daultabad Chungi) Dhanwapur road", addressFont)
                { SpacingAfter = 0, Alignment = Element.ALIGN_RIGHT });

                companyDetails.Add(new Paragraph("Gurgaon Haryana - 122006", addressFont)
                { SpacingAfter = 0, Alignment = Element.ALIGN_RIGHT });

                // ✅ Add extra bottom padding + Right Align
                companyDetails.Add(new Paragraph("GST No.: 06AACCI1709H1ZW", addressFont)
                { SpacingAfter = 40, Alignment = Element.ALIGN_RIGHT });

                companyDetails.Add(new Paragraph("", gstFont)

                { SpacingAfter = 20, Alignment = Element.ALIGN_CENTER });
                // Create PdfPCell and align it to the right
                PdfPCell textCell = new PdfPCell();
                textCell.AddElement(companyDetails);
                textCell.Border = Rectangle.NO_BORDER;
                textCell.Padding = 0;
                textCell.HorizontalAlignment = Element.ALIGN_RIGHT;  // ✅ Align cell content to right
                textCell.VerticalAlignment = Element.ALIGN_MIDDLE;

                headerTable.AddCell(textCell);



                // **2. Centered Header (Before Table)**
                Font headerCenterFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.BLACK);
                Paragraph centeredHeader = new Paragraph("Delivery Challan", headerCenterFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                };


                int challanNoInt = int.Parse(challanNo); // Convert string to int if challanNo is a string
                var ChallanData = _context.GenratedChallan.FirstOrDefault(x => x.Challan_ID == challanNoInt);


                var Partydata = _context.PartyMaster
                        .FirstOrDefault(x => x.PartyID == ChallanData.PartyID);



                // **3. New Table Before Main Table (Summary Table)**
                PdfPTable summaryTable = new PdfPTable(4); // 2 Columns
                summaryTable.WidthPercentage = 100;
                summaryTable.SetWidths(new float[] { 1f, 4f, 2f, 3f }); // Adjust column widths

                Font summaryHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.BLACK);
                Font summaryDataFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.BLACK);

                // Create the string for Challan No. conditionally
                string celldata;
                if (ChallanData.Challan_Type == 1)
                {
                    celldata = $"IAPL/IN/{ChallanData.Challan_FY}/{ChallanData.Challan_ID}";
                }
                else
                {
                    celldata = $"IAPL/OUT/{ChallanData.Challan_FY}/{ChallanData.Challan_ID}";
                }

                // Add summary rows
                summaryTable.AddCell(new PdfPCell(new Phrase("Client Details", summaryHeaderFont))
                {
                    BackgroundColor = BaseColor.LIGHT_GRAY,
                    Padding = 5,
                    Colspan = 2,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                });
                summaryTable.AddCell(new PdfPCell(new Phrase("Challan No.", summaryDataFont))
                {
                    BackgroundColor = BaseColor.WHITE,
                    Padding = 5,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                });
                summaryTable.AddCell(new PdfPCell(new Phrase(celldata, summaryHeaderFont))
                {
                    BackgroundColor = BaseColor.WHITE,
                    Padding = 5,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                });

                // Create two parts for PartyName with line break
                string dtaLine1 = Partydata.PartyName; // First part
                string dtaLine2 = Partydata.Address; // Second part (you can adjust the second part as needed)
                string dtaLine3 = Partydata.GSTNo; // Second part (you can adjust the second part as needed)

                // Create a Phrase with two Chunk elements (each representing one line)
                Phrase dtaPhrase = new Phrase();
                dtaPhrase.Add(new Chunk(dtaLine1 + "\n", summaryHeaderFont)); // First line
                dtaPhrase.Add(new Chunk(dtaLine2 + "\n", summaryDataFont)); // Second line
                dtaPhrase.Add(new Chunk(" \n", summaryDataFont)); // Second line
                dtaPhrase.Add(new Chunk("\nGSTIN : ", summaryHeaderFont)); // Second line
                dtaPhrase.Add(new Chunk(dtaLine3 + "\n", summaryDataFont)); // Second line
                dtaPhrase.Add(new Chunk("Site Name : ", summaryDataFont)); // Second line

                // Add summary rows with the Phrase containing the two lines
                summaryTable.AddCell(new PdfPCell(dtaPhrase)
                {
                    BackgroundColor = BaseColor.WHITE,
                    Padding = 5,
                    Colspan = 2,
                    Rowspan = 5
                });

                summaryTable.AddCell(new PdfPCell(new Phrase("Dated", summaryDataFont)) { BackgroundColor = BaseColor.WHITE, Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT, VerticalAlignment = Element.ALIGN_MIDDLE });
                summaryTable.AddCell(new PdfPCell(new Phrase(ChallanData.DTime.Value.ToString("dd-MM-yyyy"), summaryHeaderFont)) { BackgroundColor = BaseColor.WHITE, Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT, VerticalAlignment = Element.ALIGN_MIDDLE });

                // Add summary rows
                summaryTable.AddCell(new PdfPCell(new Phrase("Reference", summaryDataFont)) { BackgroundColor = BaseColor.WHITE, Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT, VerticalAlignment = Element.ALIGN_MIDDLE });
                summaryTable.AddCell(new PdfPCell(new Phrase("", summaryHeaderFont)) { BackgroundColor = BaseColor.WHITE, Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT, VerticalAlignment = Element.ALIGN_MIDDLE });

                // Add summary rows
                summaryTable.AddCell(new PdfPCell(new Phrase("Ref. Date", summaryDataFont)) { BackgroundColor = BaseColor.WHITE, Padding = 10, Rowspan = 3, HorizontalAlignment = Element.ALIGN_RIGHT, VerticalAlignment = Element.ALIGN_MIDDLE });
                summaryTable.AddCell(new PdfPCell(new Phrase("", summaryHeaderFont)) { BackgroundColor = BaseColor.WHITE, Padding = 3, HorizontalAlignment = Element.ALIGN_RIGHT, VerticalAlignment = Element.ALIGN_MIDDLE });





                // **4. Data Table Section**
                PdfPTable table = new PdfPTable(4); // Adjust column count as per database
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 1f, 4f, 2f, 3f }); // Adjust column widths

                // **Table Headers**
                Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);
                PdfPCell cell1 = new PdfPCell(new Phrase("Sr. No.", headerFont)) { BackgroundColor = BaseColor.LIGHT_GRAY, Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE };
                PdfPCell cell2 = new PdfPCell(new Phrase("Description Of Goods", headerFont)) { BackgroundColor = BaseColor.LIGHT_GRAY, Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE };
                PdfPCell cell3 = new PdfPCell(new Phrase("QTY", headerFont)) { BackgroundColor = BaseColor.LIGHT_GRAY, Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE };
                PdfPCell cell4 = new PdfPCell(new Phrase("Remark", headerFont)) { BackgroundColor = BaseColor.LIGHT_GRAY, Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE };

                table.AddCell(cell1);
                table.AddCell(cell2);
                table.AddCell(cell3);
                table.AddCell(cell4);

                // **3. Fetch Data from Database**
                var challans = _context.GenratedChallan.Where(x=>x.Challan_ID == challanNoInt).ToList(); // Replace with your table name
                Font dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.BLACK);
                int i = 1;
                int maxRows = 10;

                // Loop through the challans
                foreach (var item in challans.Take(maxRows))
                {
                    table.AddCell(new PdfPCell(new Phrase(i.ToString(), dataFont)) { Padding = 5, VerticalAlignment = Element.ALIGN_MIDDLE });
                    table.AddCell(new PdfPCell(new Phrase(item.DescriptionOfGoods, dataFont)) { Padding = 5, VerticalAlignment = Element.ALIGN_MIDDLE });
                    table.AddCell(new PdfPCell(new Phrase(item.Qty.ToString(), dataFont)) { Padding = 5, VerticalAlignment = Element.ALIGN_MIDDLE });
                    table.AddCell(new PdfPCell(new Phrase(item.Remark, dataFont)) { Padding = 5, VerticalAlignment = Element.ALIGN_MIDDLE });
                    i++;
                }

                //// Add empty rows if there are fewer than 10 rows
                //int rowsToAdd = maxRows - challans.Count();
                //for (int j = 0; j < rowsToAdd; j++)
                //{
                //    table.AddCell(new PdfPCell(new Phrase(" ", dataFont)) { Padding = 5 });
                //    table.AddCell(new PdfPCell(new Phrase(" ", dataFont)) { Padding = 5 });
                //    table.AddCell(new PdfPCell(new Phrase(" ", dataFont)) { Padding = 5 });
                //    table.AddCell(new PdfPCell(new Phrase(" ", dataFont)) { Padding = 5 });
                //}







                // ... Header table code here

                // Add the header table, centered header, summary, and data tables
                document.Add(headerTable);
                document.Add(centeredHeader);
                document.Add(summaryTable);
                document.Add(table);

                // Content Section
                Font contentFont = FontFactory.GetFont(FontFactory.HELVETICA, 11, BaseColor.BLACK);
                Font contentFont2 = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);
                document.Add(new Paragraph("\n")); // Space after header
                document.Add(new Paragraph($"Total Amount (Commercial Value) :                  0.00", summaryHeaderFont));
                document.Add(new Paragraph("\n")); // Space after header

                document.Add(new Paragraph("Goods received in good condition :                  YES/NO", summaryHeaderFont));
                document.Add(new Paragraph("\n")); // Space after header
                document.Add(new Paragraph("\n")); // Space after header

                document.Add(new Paragraph("Receiver's signature with company stamp", summaryHeaderFont));
                document.Add(new Paragraph("\n")); // Space after header
                document.Add(new Paragraph("\n")); // Space after header

                document.Add(new Paragraph("for INTELLIGENT AUTOCONTROL PVT LTD", contentFont2));

                // Create a table to arrange the logo and text side by side
                PdfPTable table11 = new PdfPTable(1);
                table11.WidthPercentage = 100; // Make the table take full width
                table11.SetWidths(new float[] { 3f }); // Set column widths, logo on the left, text on the right

                // Left side: Company Logo
                string logoPath2 = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/stamp.png");
                if (System.IO.File.Exists(logoPath2))
                {
                    Image logo = Image.GetInstance(logoPath2);
                    logo.ScaleToFit(75, 75); // Resize the logo
                    PdfPCell logoCell = new PdfPCell(logo)
                    {
                        Border = Rectangle.NO_BORDER,
                        Padding = 0,
                        PaddingTop = 3f,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table11.AddCell(logoCell); // Add logo cell to the table
                }
                else
                {
                    table11.AddCell(new PdfPCell(new Phrase("Logo Missing")) { Border = Rectangle.NO_BORDER });
                }
                // Right side: Authorized Signatory
                PdfPCell textCell2 = new PdfPCell(new Phrase("Authorised Signatory", contentFont))
                {
                    Border = Rectangle.NO_BORDER,
                    VerticalAlignment = Element.ALIGN_BOTTOM
                };
                table11.AddCell(textCell2); // Add text cell to the table

                document.Add(table11); // Add the table to the document



                document.Close();
                return memoryStream.ToArray();
            }
        }
        public class Footer : PdfPageEventHelper
        {
            // This will be triggered on each page
            public override void OnEndPage(PdfWriter writer, Document document)
            {
                base.OnEndPage(writer, document);

                // Footer Font
                Font footerFont = FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.BLACK);
                Font footerFont2 = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.BLACK);

                // Get the current page size
                Rectangle pageSize = document.PageSize;

                // Footer Content (can be dynamic, for now static)
                string footerContent = "Regd. Office : ";
                string footerContent2 = "WW-23, FirstFloor, Maliby Towne, Sohna road, Gurugram - 122018 (Haryana)";

                // Create the footer Phrase objects
                Phrase footerPhrase = new Phrase(footerContent, footerFont);
                Phrase footerPhrase2 = new Phrase(footerContent2, footerFont2);

                // Create the footer positions for each line (Y positions will be offset)
                float footerX = (pageSize.GetLeft(0) + pageSize.GetRight(0)) / 2;  // Centered horizontally
                float footerY1 = pageSize.GetBottom(0) + 20; // First line Y position
                float footerY2 = footerY1 - 12; // Second line Y position (12 units above the first line)

                // Write the first line of the footer
                ColumnText.ShowTextAligned(writer.DirectContent, Element.ALIGN_CENTER, footerPhrase, footerX, footerY1, 0);

                // Write the second line of the footer
                ColumnText.ShowTextAligned(writer.DirectContent, Element.ALIGN_CENTER, footerPhrase2, footerX, footerY2, 0);
            }
        }

    }
}

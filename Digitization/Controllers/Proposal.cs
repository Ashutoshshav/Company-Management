using Digitization.Models;
using Digitization.Services;
using Digitization.ViewModel;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Digitization.Controllers
{
    public class Proposal : Controller
    {
        private readonly ApplicationDBContext _context;

        public Proposal(ApplicationDBContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> SearchParty(string term)
        {
            var suggestions = await _context.PartyMaster
                .Where(p => p.PartyName.Contains(term))
                .Select(p => p.PartyName)
                .Take(5)
                .ToListAsync();

            return Json(suggestions);
        }
        [HttpGet]
        public async Task<IActionResult> GetContactPersons(string partyName)
        {
            var contacts = await _context.PartyMaster
                .Where(c => c.PartyName == partyName)
                .Select(c => c.ContactPerson)
                .Take(5)
                .ToListAsync();

            return Json(contacts);
        }
        [HttpGet]
        public async Task<IActionResult> GetAddresses(string partyName, string contactPerson)
        {
            var addresses = await _context.PartyMaster
                .Where(a => a.PartyName == partyName && a.ContactPerson == contactPerson)
                .Select(a => a.Address)
                .ToListAsync();

            return Json(addresses);
        }
        [HttpGet]
        public async Task<IActionResult> GetMobileNumber(string partyName, string contactPerson, string address)
        {
            var mobileNo = await _context.PartyMaster
                .Where(c => c.PartyName == partyName && c.ContactPerson == contactPerson)
                .Select(c => c.ContactNo)
                .FirstOrDefaultAsync();

            return Json(mobileNo);
        }





        [HttpPost]
        public IActionResult SaveData([FromBody] SaveProposalData request)
        {
            if (request.Items == null || !request.Items.Any())
            {
                return BadRequest("No data provided.");
            }
            var data = _context.GenerateProposal.OrderByDescending(x => x.Proposal_ID).FirstOrDefault();
            var data2 = _context.PartyMaster.Where(x => x.PartyName == request.PartyName && x.ContactPerson == request.Person).FirstOrDefault();


            string ProposalFY = GetFinancialYear(DateTime.Now.ToString());
            string ProposalNo = "";

            foreach (var item in request.Items)
            {
                var generatedProposal = new GenerateProposal
                {
                    Proposal_ID = data.Proposal_ID + 1, // Generate unique Proposal_ID
                    DTime = DateTime.Now,
                    PartyID = data2.PartyID,
                    SenderName = request.SenderName,
                    Proposal_FY = ProposalFY,
                    Subject = request.Subject,
                    ItemDescription = item.ItemDescription,
                    HSN_SAC_NO = item.HSN_SAC_NO,
                    Unit_Rate = item.Unit_Rate,
                    Amount = item.Amount,
                    QTY = item.Qty,
                    DeliveryTime = item.DeliveryTime
                };

                _context.GenerateProposal.Add(generatedProposal);
            }

            _context.SaveChanges();

            return Ok(new { ProposalNo = $"IAPL/{ProposalFY}/{data.Proposal_ID + 1}", realProposalNo = data.Proposal_ID + 1 });
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
                    logo.ScaleToFit(100, 100); // Resize the logo
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





                // Add the header table, centered header, summary, and data tables
                document.Add(headerTable);


               

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
                float footerY1 = pageSize.GetBottom(0) + 30; // First line Y position
                float footerY2 = footerY1 - 12; // Second line Y position (12 units above the first line)

                // Write the first line of the footer
                ColumnText.ShowTextAligned(writer.DirectContent, Element.ALIGN_CENTER, footerPhrase, footerX, footerY1, 0);

                // Write the second line of the footer
                ColumnText.ShowTextAligned(writer.DirectContent, Element.ALIGN_CENTER, footerPhrase2, footerX, footerY2, 0);
            }
        }

    }
}

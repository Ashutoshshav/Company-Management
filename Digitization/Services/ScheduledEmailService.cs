using Digitization.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MimeKit;
using MailKit.Net.Smtp;
using Digitization.Models;
using System.Text;

public class ScheduledEmailService : BackgroundService
{
    private readonly EmailService _emailService;
    private readonly IServiceScopeFactory _scopeFactory;

    public ScheduledEmailService(EmailService emailService, IServiceScopeFactory scopeFactory)
    {
        _emailService = emailService;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var scheduledTime1 = new DateTime(now.Year, now.Month, now.Day, 9, 35, 0);
            var scheduledTime2 = new DateTime(now.Year, now.Month, now.Day, 20, 30, 0);

            if ((now >= scheduledTime1 && now < scheduledTime1.AddMinutes(1)) ||
                (now >= scheduledTime2 && now < scheduledTime2.AddMinutes(1)))
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

                    var devicesStsCount = await _context.DailyEmployeeEntry.FirstOrDefaultAsync();

                    using var package = new ExcelPackage();
                    var worksheet1 = package.Workbook.Worksheets.Add("ATTENDANCE");

                    // Merge cells for the title in the first row
                    worksheet1.Cells["A1:F1"].Merge = true;
                    worksheet1.Cells["A1:F1"].Value = $"ATTENDANCE OF DATE {now.Date:dd MMM yyyy}";
                    worksheet1.Cells["A1:F1"].Style.Font.Bold = true;
                    worksheet1.Cells["A1:F1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet1.Cells["A1:F1"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                    // Add headers to the worksheet1
                    worksheet1.Cells[2, 1].Value = "USER NAME";
                    worksheet1.Cells[2, 2].Value = "TIME IN";
                    worksheet1.Cells[2, 3].Value = "ATTENDANCE TYPE";
                    worksheet1.Cells[2, 4].Value = "TIME OUT";
                    worksheet1.Cells[2, 5].Value = "WORK DURATION";
                    worksheet1.Cells[2, 6].Value = "ENTRY DATE TIME";

                    // Style of headers of the worksheet1
                    worksheet1.Cells[2, 1].Style.Font.Bold = true;
                    worksheet1.Cells[2, 2].Style.Font.Bold = true;
                    worksheet1.Cells[2, 3].Style.Font.Bold = true;
                    worksheet1.Cells[2, 4].Style.Font.Bold = true;
                    worksheet1.Cells[2, 5].Style.Font.Bold = true;
                    worksheet1.Cells[2, 6].Style.Font.Bold = true;

                    var attendanceList = await _context.DailyEmployeeEntry
                        .Where(d => d.TimeIn.Value.Date == DateTime.Today)
                        .ToListAsync();

                    int row = 3;

                    if (attendanceList != null)
                    {
                        foreach (var item in attendanceList)
                        {
                            worksheet1.Cells[row, 1].Value = item.EmployeeName;
                            worksheet1.Cells[row, 2].Value = item.TimeIn;
                            worksheet1.Cells[row, 3].Value = item.InEntryType;
                            worksheet1.Cells[row, 4].Value = item.TimeOut;
                            worksheet1.Cells[row, 5].Value = item.WorkingDuration;
                            worksheet1.Cells[row, 6].Value = item.EntryDTime;

                            row++;
                        }
                    }

                    // AutoFit columns for better visibility
                    worksheet1.Cells[worksheet1.Dimension.Address].AutoFitColumns();

                    // Create a memory stream to store the file content
                    var stream = new MemoryStream();
                    package.SaveAs(stream);

                    stream.Position = 0; // Reset stream position

                    string[] emails = { "mukesh@icona.in" };

                    foreach(var email in emails)
                    {
                        await _emailService.SendEmailAsync(
                            $"{email}",
                            "Attendance",
                            GenerateEmailBody((now.Date).ToString("dd MMM yyyy"), attendanceList),
                            stream.ToArray(), // Pass Excel file as byte array
                            $"ATTENDANCE_OF_{now.Date:dd MMM yyyy}.xlsx"
                        );
                    }
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Check every 1 minute
        }
    }

    public string GenerateEmailBody(string date, List<DailyEmployeeEntry> attendanceList)
    {
        StringBuilder tableRows = new StringBuilder();

        foreach (var item in attendanceList)
        {
            tableRows.Append($@"
            <tr>
                <td>{item.EmployeeName}</td>
                <td>{item.TimeIn}</td>
                <td>{item.InEntryType}</td>
                <td>{item.TimeOut}</td>
            </tr>");
        }

        return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; }}
                    .container {{ padding: 20px; border: 1px solid #ddd; border-radius: 8px; width: 80%; margin: auto; }}
                    .header {{ font-size: 18px; font-weight: bold; color: #333; }}
                    .table {{ width: 100%; border-collapse: collapse; margin-top: 10px; }}
                    .table th, .table td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
                    .table th {{ background-color: #007bff; color: white; }}
                    .footer {{ margin-top: 20px; font-size: 12px; color: #555; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <p class='header'>📅 Date: {date}</p>
                    <p><strong>Attendance</strong></p>
                    <table class='table'>
                        <tr>
                            <th>User Name</th>
                            <th>Time In</th>
                            <th>Attendance Type</th>
                            <th>Time Out</th>
                        </tr>
                        {tableRows}
                    </table>
                    <p class='footer'>📎 Attachment: ATTENDANCE OF {date.Replace(" ", "_")}.xlsx</p>
                </div>
            </body>
            </html>";
    }
}

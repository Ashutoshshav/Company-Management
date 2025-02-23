using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Digitization.Models;
using Digitization.Services;
using Digitization.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Digitization.Controllers
{
    [Authorize(Policy = "SharedAccess")]
    public class HomeController : Controller
    {
        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _configuration;

        public HomeController(ApplicationDBContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        //var query = from emp in _context.EmployeeMaster
        //            join att in _context.DailyEmployeeEntry
        //                on new { emp.EmployeeID, Date = today } equals new { att.EmployeeID, Date = att.TimeIn }
        //                into attendanceGroup
        //            group new { emp, attendanceGroup } by emp.JobCategory into grouped
        //            select new
        //            {
        //                JobCategory = grouped.Key,
        //                TotalEmployees = grouped.Count(),
        //                PresentCount = grouped.Sum(g => g.attendanceGroup.Count()),
        //                AbsentCount = grouped.Count() - grouped.Sum(g => g.attendanceGroup.Count())
        //            };
        //var result = query.ToList();
        //var employeesWithoutEntries = await _context.EmployeeMaster
        //    .Where(em => _context.DailyEmployeeEntry
        //        .Any(de => de.EmployeeID == em.EmployeeID && de.TimeIn != null && de.TimeIn.Value.Date == today))
        //    .ToListAsync();
        public async Task<IActionResult> ChartData()
        {
            var employeeId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine(employeeId + "EmployeeID");

            int DaysAgo = 10;
            var varDaysAgo = DateTime.UtcNow.AddDays((-DaysAgo));
            var today = DateTime.UtcNow;

            int totalPresence = await _context.DailyEmployeeEntry
                .Where(e => e.EmployeeID == employeeId && e.TimeIn >= varDaysAgo && e.TimeIn < today)
                .CountAsync();

            // Fetch total work duration and sum it up
            var totalWorkDurationList = await _context.DailyEmployeeEntry
                .Where(e => e.EmployeeID == employeeId && e.TimeIn >= varDaysAgo && e.TimeIn < today)
                .Select(e => e.WorkingDuration)
                .ToListAsync();

            // Convert "hh:mm:ss" strings to TimeSpan and sum them
            TimeSpan totalWorkDuration = totalWorkDurationList
                .Where(d => !string.IsNullOrEmpty(d)) // Ensure non-null values
                .Select(d => TimeSpan.TryParse(d, out var ts) ? ts : TimeSpan.Zero)
                .Aggregate(TimeSpan.Zero, (sum, next) => sum + next);

            // Convert total duration to readable format
            string formattedTotalDuration = $"{(int)totalWorkDuration.TotalHours:D2}:{totalWorkDuration.Minutes:D2}:{totalWorkDuration.Seconds:D2}";

            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var workData = await _context.ProjectBaseWorking
                .Where(p => p.EmployeeID == employeeId &&
                            p.StartDTime >= startOfMonth &&
                            p.StartDTime <= endOfMonth)
                .GroupBy(p => new
                {
                    p.RecordID,
                    p.EmployeeID,
                    Date = p.StartDTime.Date,
                    p.ProjectID
                })
                .Select(g => new
                {
                    RecordID = g.Key.RecordID,
                    EmployeeID = g.Key.EmployeeID,
                    EndDTime = g.Max(p => p.EndDTime),
                    Description = g.Max(p => p.Description),
                    StartDTime = g.Key.Date,
                    ProjectID = g.Key.ProjectID,
                    WorkDurations = g.Select(p => p.WorkDuration) // Get all durations as string
                })
                .ToListAsync(); // Execute query and get results into memory


            var currentMonthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var currentMonthEnd = currentMonthStart.AddMonths(1).AddDays(-1);

            var projectWorkData = _context.ProjectBaseWorking
                .Where(pb => pb.EmployeeID == employeeId &&  // Filter by EmployeeID in ProjectBaseWorking
                             pb.StartDTime >= currentMonthStart &&
                             pb.StartDTime <= currentMonthEnd)   // Filter by current month
                .GroupBy(pb => pb.ProjectID) // Group by ProjectID
                .Select(g => new
                {
                    ProjectID = g.Key,
                    ProjectName = _context.ProjectMaster
                        .Where(pm => pm.ProjectId == g.Key) // Get project name from ProjectMaster
                        .Select(pm => pm.ProjectName)
                        .FirstOrDefault(),
                    WorkData = g.ToList()
                })
                .ToList();

            var data = new { totalPresence, absent = (DaysAgo - totalPresence) };

            var tasks = await _context.WorkMaster
                .Where(w => w.EmployeeID == employeeId && w.EndDtime == null && w.CompleatingDuration == null)
                .ToListAsync();

            // Fetch and combine data from TravelExpenses and OtherExpenses
            var PaymentApprovalList = await _context.TravelExpenses
                .Where(o => o.EmployeeID == employeeId)
                .Select(t => new
                {
                    ExpenseType = "Travel",
                    Amount = t.Amount,
                    ApprovalStatus = t.ApprovalStatus,
                })
                .Union(
                    _context.OtherExpenses
                    .Where(o => o.EmployeeID == employeeId)
                    .Select(o => new
                    {
                        ExpenseType = o.ExpenseType,
                        Amount = o.Amount,
                        ApprovalStatus = o.ApprovalStatus,
                    })
                )
                .ToListAsync();

            var performanceData = await _context.UserPerformance
            .Where(up => up.EmployeeID == employeeId)
            .Join(
                _context.PerformanceTypes,
                up => up.PerformanceID,
                pt => pt.PerformanceID,
                (up, pt) => new
                {
                    pt.PerformanceType,
                    up.PerformanceScore
                }
            ).ToListAsync();

            var appliedLeaves = await _context.UserLeaves
                .Where(w => w.UserID == employeeId)
                .Select(l => new
                {
                    l.RecordID,
                    l.CreatedAt,
                    l.Reason,
                    Status = l.Status ?? "Pending",  // Default value if NULL
                    ApprovedBy = l.ApprovedBy ?? "N/A" // Default value if NULL
                })
                .ToListAsync();

            return Json(new { tasks, PaymentApprovalList, data = data, totalWorkingHours = formattedTotalDuration, projectWorkData = projectWorkData, performanceData, appliedLeaves });
        }

        public IActionResult Home()
        {
            // Get all Job Categories
            var jobCategories = _context.AttendanceEntryType
                .Select(j => j.InEntryText)
                .ToList() ?? new List<string>(); // Ensures it's never null

            // Get today's attendance
            var today = DateTime.Today;
            var attendanceData = _context.DailyEmployeeEntry
                .Where(d => d.TimeIn.HasValue && d.TimeIn.Value.Date == today)
                .Select(d => new
                {
                    d.EmployeeID,
                    InEntryText = d.InEntryType ?? "Unknown",
                    TimeIn = d.TimeIn
                })
                .ToList();

            // Get all employees
            var allEmployees = _context.EmployeeMaster
                .Select(e => new
                {
                    EmployeeID = e.EmployeeID ?? "Unknown",
                    EmployeeName = e.EmployeeName ?? "Unknown"
                })
                .ToList();

            // Find employees who marked attendance
            var presentEmployees = attendanceData.Select(d => d.EmployeeID).Distinct().ToList();

            // Find employees who did NOT mark attendance (Absent WO/I)
            var absentWithoutInfo = allEmployees
                .Where(e => !attendanceData.Any(d => d.EmployeeID == e.EmployeeID))
                .Select(e => new EmployeeAttendanceDetails
                {
                    EmployeeName = e.EmployeeName,
                    JobCategory = "Absent WO/I",
                    TimeIn = "N/A"
                })
                .ToList();

            // Calculate presence count per Job Category
            var presenceCountPerJobCategory = attendanceData
                .GroupBy(d => d.InEntryText)
                .ToDictionary(g => g.Key, g => g.Count());

            // Calculate total present and absent counts
            var totalPresentCount = presentEmployees.Count();
            var totalAbsentCount = allEmployees.Count() - totalPresentCount;

            var totalAttendanceCount = totalPresentCount + totalAbsentCount;
            // Construct chart data with presence count for each Job Category
            var chartData = jobCategories.Select(jobCategory => new AttendanceChartViewModel
            {
                JobCategory = jobCategory ?? "Unknown",
                PresentCount = presenceCountPerJobCategory.ContainsKey(jobCategory) ? presenceCountPerJobCategory[jobCategory] : 0, // Add presence count for each category
                Percentage = totalAttendanceCount > 0 ?
                    (double)(presenceCountPerJobCategory.ContainsKey(jobCategory) ? presenceCountPerJobCategory[jobCategory] : 0) / totalAttendanceCount * 100
                    : 0,
                AttendanceDetails = attendanceData
                    .Where(d => d.InEntryText == jobCategory)
                    .Select(d => new EmployeeAttendanceDetails
                    {
                        EmployeeName = allEmployees.FirstOrDefault(e => e.EmployeeID == d.EmployeeID)?.EmployeeName ?? "Unknown",
                        JobCategory = d.InEntryText,
                        TimeIn = d.TimeIn.HasValue ? d.TimeIn.Value.ToString("dd-MM-yyyy HH:mm:ss") : "N/A"
                    })
                    .ToList()
            }).ToList();


            // Add "Absent WO/I" category explicitly
            chartData.Add(new AttendanceChartViewModel
            {
                JobCategory = "Absent WO/I",
                AttendanceDetails = absentWithoutInfo
            });

            // Prepare final result
            var result = new
            {
                TotalPresentCount = totalPresentCount,
                TotalAbsentCount = totalAbsentCount,
                ChartData = chartData
            };

            // Calculate percentages for each job category
            var chartDatas = jobCategories.Select(jobCategory => new
            {
                JobCategory = jobCategory ?? "Unknown",
                PresenceCount = presenceCountPerJobCategory.ContainsKey(jobCategory) ? presenceCountPerJobCategory[jobCategory] : 0,
                Percentage = totalAttendanceCount > 0 ?
                    (double)(presenceCountPerJobCategory.ContainsKey(jobCategory) ? presenceCountPerJobCategory[jobCategory] : 0) / totalAttendanceCount * 100
                    : 0
            }).ToList();

            // Prepare data for pie chart
            var pieChartData = chartData.Select(c => c.PresentCount).ToList();
            var pieChartLabels = chartData.Select(c => c.JobCategory).ToList();
            var pieChartPercentages = chartData.Select(c => c.Percentage).ToList();

            // Prepare result object
            var resultData = new
            {
                PieChartData = pieChartData,
                PieChartLabels = pieChartLabels,
                PieChartPercentages = pieChartPercentages
            };

            return Json(result);
            //return View(result);
        }

        public async Task<IActionResult> checkEmployeeLogSts()
        {
            var employeeId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var today = DateTime.Today;

            var sts = await _context.DailyEmployeeEntry
                .Where(e => e.EmployeeID == employeeId && EF.Functions.DateDiffDay(e.TimeIn, today) == 0)
                .Select(e => e.LoggedStatus)
                .FirstOrDefaultAsync();

            Console.WriteLine(sts);
            return Json(sts);
        }

        public async Task<IActionResult> GetAllAttendanceType()
        {
            var attendanceEntryTypes = await _context.AttendanceEntryType.ToListAsync();
            return Json(attendanceEntryTypes);
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(EmployeeMaster Employee)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.EmployeeMaster.FirstOrDefaultAsync(e => e.EmployeeEmail == Employee.EmployeeEmail);

                if (user != null)
                {
                    if (user.EmployeePassword == Employee.EmployeePassword)
                    {
                        // Create claims
                        var claims = new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.EmployeeID),
                            new Claim(ClaimTypes.Role, user.EmployeeRole),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                        };

                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        var token = new JwtSecurityToken(
                            issuer: _configuration["Jwt:Issuer"],
                            audience: _configuration["Jwt:Audience"],
                            claims: claims,
                            expires: DateTime.UtcNow.AddDays(1), // Reduced expiration time
                            signingCredentials: creds
                        );

                        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                        // Store token in cookies securely (ensure using HTTPS)
                        Response.Cookies.Append("Token", tokenString, new CookieOptions
                        {
                            SameSite = SameSiteMode.Strict,
                            Secure = false, // Set to true if using HTTPS
                            Expires = DateTime.UtcNow.AddDays(1) // Adjust expiration to match the token
                        });

                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid Password");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "User doesn't exist");
                }
            }

            return View(Employee);
        }

        public async Task<IActionResult> GetProjectData()
        {
            var dropDownData = await _context.ProjectMaster.ToListAsync();

            return Ok(dropDownData);
        }

        [HttpPost]
        public async Task<IActionResult> EnterDailyEmployeeEntry([FromBody] Dictionary<string, string> data)
        {
            try
            {
                var RecordID = data["RecordID"];
                var InEntryText = data["InEntryText"];
                var employeeId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var employeeName = await _context.EmployeeMaster
                                .Where(e => e.EmployeeID == employeeId)
                                .Select(e => e.EmployeeName)
                                .FirstOrDefaultAsync();

                DateTime utcNow = DateTime.UtcNow;
                TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                DateTime istTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, istZone);

                var employeeDailyEntry = new DailyEmployeeEntry
                {
                    EmployeeID = employeeId,
                    EmployeeName = employeeName,
                    InEntryType = InEntryText,
                    TimeIn = istTime,
                    LoggedStatus = "LoggedIn",
                };

                _context.DailyEmployeeEntry.Add(employeeDailyEntry);
                await _context.SaveChangesAsync();

                Console.WriteLine(RecordID + InEntryText + employeeId + employeeName);

                return Ok(new { Message = "Entry successfully saved" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmPassword([FromBody] PasswordConfirmationModel model)
        {
            try
            {
                var employeeId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                bool isPasswordValid = await _context.EmployeeMaster
                    .AnyAsync(e => e.EmployeeID == employeeId && e.EmployeePassword == model.Password);

                Console.WriteLine("password" + isPasswordValid);
                // Validate the password (replace with your actual logic)
                if (isPasswordValid)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        public async Task<IActionResult> DailyEmployeeTimeOut()
        {
            try
            {
                var employeeId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var today = DateTime.Today;

                var isLoggedIn = await _context.DailyEmployeeEntry
                    .Where(e => e.EmployeeID == employeeId && EF.Functions.DateDiffDay(e.TimeIn, today) == 0)
                    .FirstOrDefaultAsync();

                if (isLoggedIn != null)
                {
                    isLoggedIn.TimeOut = DateTime.Now;

                    var duration = isLoggedIn.TimeOut.Value - isLoggedIn.TimeIn;
                    TimeSpan timeSpan = TimeSpan.Parse(duration.ToString());
                    var durationString = duration.ToString();

                    Console.WriteLine(timeSpan.Hours);
                    Console.WriteLine(timeSpan.Minutes);
                    isLoggedIn.LoggedStatus = "LoggedOut";
                    isLoggedIn.WorkingDuration = $"{timeSpan.Hours}:{timeSpan.Minutes}:{timeSpan.Seconds}";
                    // Save changes to the database
                    await _context.SaveChangesAsync();

                    return Json(new { success = true });
                }

                return Json(new { success = false });
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        public async Task<IActionResult> ProjectLogin(ProjectBaseWorking projectBaseWorking)
        {
            if (ModelState.IsValid)
            {
                await _context.ProjectBaseWorking.AddAsync(projectBaseWorking);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Something Went Wrong");

            return RedirectToAction("Index", "Home");
        }

        public IActionResult LoadLeaveForm()
        {
            return PartialView("_ApplyLeaveForm"); // Pass it to the partial view
        }

        [HttpPost]
        public IActionResult ApplyLeave(UserLeaves userLeaves)
        {
            var employeeId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            userLeaves.UserID = employeeId; // Assign user ID manually

            // Remove validation for UserID and Description
            ModelState.Remove("UserID");
            ModelState.Remove("Description");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(e => e.Value.Errors.Count > 0)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray());

                return Json(new { success = false, errors });
            }

            // Save data to the database
            _context.UserLeaves.Add(userLeaves);
            _context.SaveChanges();

            return Json(new { success = true, message = "Leave Applied Successfully!" });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class PasswordConfirmationModel
    {
        public string Password { get; set; }
    }
}

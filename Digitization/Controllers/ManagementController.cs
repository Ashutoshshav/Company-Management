using System.Security.Claims;
using Azure.Core;
using Dapper;
using Digitization.Models;
using Digitization.Services;
using Digitization.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Digitization.Controllers
{
    //[Authorize(Policy = "ManagementAccess")]
    public class ManagementController : Controller
    {
        private readonly ApplicationDBContext _context;

        public ManagementController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: ManagementController
        public async Task<ActionResult> PaymentApprovalList()
        {
            // Fetch and combine data from TravelExpenses and OtherExpenses
            var PaymentApprovalList = await _context.TravelExpenses
                .Select(t => new PaymentApprovalViewModel
                {
                    RecordID = t.RecordID,
                    ProjectID = t.ProjectID,
                    EmployeeID = t.EmployeeID,
                    ExpenseType = "Travel",
                    Amount = t.Amount,
                    ExpenseDate = t.TravelDate,
                    EntryDTime = t.EntryDTime,
                    ApprovalStatus = t.ApprovalStatus,
                })
                .Union(
                    _context.OtherExpenses
                    .Select(o => new PaymentApprovalViewModel
                    {
                        RecordID = o.RecordID,
                        ProjectID = o.ProjectID,
                        EmployeeID = o.EmployeeID,
                        ExpenseType = o.ExpenseType,
                        Amount = o.Amount,
                        ExpenseDate = o.ExpenseDate,
                        EntryDTime = o.EntryDTime,
                        ApprovalStatus = o.ApprovalStatus,
                    })
                )
                .ToListAsync();

            return View(PaymentApprovalList);
        }

        // GET: ManagementController/Details/5
        public async Task<ActionResult> Details(int RecordID, string ExpenseType)
        {
            Console.WriteLine(ExpenseType + "   " + RecordID);
            if (string.IsNullOrEmpty(ExpenseType))
            {
                return BadRequest("Expense type is required.");
            }

            object expenseDetails;

            if (ExpenseType == "Travel")
            {
                Console.WriteLine(ExpenseType + "   " + RecordID);
                expenseDetails = await _context.TravelExpenses.FirstOrDefaultAsync(te => te.RecordID == RecordID);
                if (expenseDetails == null)
                {
                    return NotFound("Travel expense not found.");
                }

                return View("TravelExpenseDetails", expenseDetails);
            }
            else
            {
                Console.WriteLine(ExpenseType + "   " + RecordID);
                expenseDetails = await _context.OtherExpenses.FirstOrDefaultAsync(oe => oe.RecordID == RecordID);
                if (expenseDetails == null)
                {
                    return NotFound("Other expense not found.");
                }

                return View("OtherExpenseDetails", expenseDetails);
            }
        }

        [PermissionAuthorize("ReadEmpExpList")]
        public async Task<ActionResult> EmployeesExpenseList()
        {
            var EmployeeExpenseList = await _context.EmployeeMaster
            .OrderBy(e => e.EmployeeID)
            .Select(e => new EmployeeExpenseViewModel
            {
                EmployeeID = e.EmployeeID,
                EmployeeName = e.EmployeeName,
                TotalExpense = (
                    _context.OtherExpenses
                    .Where(oe => oe.EmployeeID == e.EmployeeID)
                    .Sum(oe => oe.Amount)
                    +
                    _context.TravelExpenses
                    .Where(te => te.EmployeeID == e.EmployeeID)
                    .Sum(te => te.Amount)
                )
            })
            .ToListAsync();

            EmployeeExpenseList = EmployeeExpenseList
                .OrderBy(emp => int.TryParse(emp.EmployeeID, out int id) ? id : int.MaxValue)
                .ToList();

            foreach (var item in EmployeeExpenseList)
            {
                Console.WriteLine(item.EmployeeID + "EmployeeID");
            }

            return View(EmployeeExpenseList);
        }

        [PermissionAuthorize("ReadEmpExpDetails")]
        public async Task<IActionResult> EmployeeExpenseDetails(string EmployeeID)
        {
            Console.WriteLine("EmployeeID" + "   " + EmployeeID);
            var employeeDetails = await _context.EmployeeMaster
                .Where(e => e.EmployeeID == EmployeeID)
                .Select(e => new EmployeeExpenseDetailsViewModel
                {
                    EmployeeID = e.EmployeeID,
                    EmployeeName = e.EmployeeName,
                    PaymentApprovalViewModel = _context.OtherExpenses
                        .Where(o => o.EmployeeID == e.EmployeeID)
                        .Select(o => new PaymentApprovalViewModel
                        {
                            RecordID = o.RecordID,
                            ProjectID = o.ProjectID,
                            EmployeeID = o.EmployeeID,
                            ExpenseType = o.ExpenseType,
                            Amount = o.Amount,
                            ExpenseDate = o.ExpenseDate,
                            EntryDTime = o.EntryDTime,
                            ApprovalStatus = o.ApprovalStatus,
                        })
                        .Union(
                            _context.TravelExpenses
                                .Where(t => t.EmployeeID == e.EmployeeID)
                                .Select(t => new PaymentApprovalViewModel
                                {
                                    RecordID = t.RecordID,
                                    ProjectID = t.ProjectID,
                                    EmployeeID = t.EmployeeID,
                                    ExpenseType = "Travel",
                                    Amount = t.Amount,
                                    ExpenseDate = t.TravelDate,
                                    EntryDTime = t.EntryDTime,
                                    ApprovalStatus = t.ApprovalStatus,
                                })
                        )
                        .ToList()
                })
                .FirstOrDefaultAsync();

            foreach (var expense in employeeDetails.PaymentApprovalViewModel)
            {
                Console.WriteLine($"RecordID: {expense.RecordID}, ApprovalStatus: {expense.ApprovalStatus}");
            }

            if (employeeDetails == null) return NotFound();

            return Json(employeeDetails);
        }

        //[HttpPost]
        //public IActionResult UpdateApproval(ApprovalViewModel model)
        //{
        //    //if (!ModelState.IsValid)
        //    //{
        //    //    // Return the form with validation errors
        //    //    return View(model);
        //    //}

        //    var employeeId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    Console.WriteLine(model.ExpenseType + "   " + model.RecordID + "UpdateApproval");

        //    // Process the approval based on the ExpenseType
        //    if (model.ExpenseType == "Travel")
        //    {
        //        Console.WriteLine(model.ExpenseType + "   " + model.RecordID + "Travel");
        //        // Fetch the TravelExpenses record
        //        var travelExpense = _context.TravelExpenses.Find(model.RecordID);
        //        if (travelExpense == null) return NotFound();

        //        // Update fields
        //        travelExpense.ApprovalStatus = model.ApprovalStatus;
        //        travelExpense.ApprovedBy = employeeId;
        //        travelExpense.DeductionAmount = model.DeductionAmount;
        //        travelExpense.DeductionRemark = model.DeductionRemark;
        //        travelExpense.ApprovalDate = DateTime.Now;

        //        Console.WriteLine(model.ApprovedBy);
        //        // Save changes
        //        _context.SaveChanges();

        //        return RedirectToAction("PaymentApprovalList");
        //    }
        //    else
        //    {
        //        Console.WriteLine(model.ExpenseType + "   " + model.RecordID + "else");
        //        // Fetch the OtherExpenses record
        //        var otherExpense = _context.OtherExpenses.Find(model.RecordID);
        //        if (otherExpense == null) return NotFound();

        //        // Update fields
        //        otherExpense.ApprovalStatus = model.ApprovalStatus;
        //        otherExpense.ApprovedBy = employeeId;
        //        otherExpense.DeductionAmount = model.DeductionAmount;
        //        otherExpense.DeductionRemark = model.DeductionRemark;
        //        otherExpense.ApprovalDate = DateTime.Now;

        //        Console.WriteLine(model.ApprovedBy);
        //        // Save changes
        //        _context.SaveChanges();

        //        return RedirectToAction("PaymentApprovalList");
        //    }

        //    // Redirect to a success page or reload
        //    //return RedirectToAction("PaymentApprovalList");
        //}
        
        [HttpPost]
        [PermissionAuthorize("MngEmpExpApproval")]
        public async Task<IActionResult> UpdateApproval(string EmployeeID)
        {
            var employeeId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine(EmployeeID + "   " + "UpdateApproval");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Find expense records for this employee
                var TravelExpenses = await _context.TravelExpenses
                    .Where(e => e.EmployeeID == EmployeeID)
                    .ToListAsync();

                // Update the approvals table
                var OtherExpenses = await _context.OtherExpenses
                    .Where(e => e.EmployeeID == EmployeeID)
                    .ToListAsync();

                if (!OtherExpenses.Any() && !TravelExpenses.Any())
                {
                    return NotFound(new { message = "No expenses found for this employee." });
                }
                
                // Update the expense status
                foreach (var expense in TravelExpenses)
                {
                    expense.ApprovalStatus = "Approved"; // 'Approved' or 'Rejected'
                    expense.ApprovalDate = DateTime.Now;
                    expense.ApprovedBy = employeeId;
                }
                await _context.SaveChangesAsync();

                // Update the expense status
                foreach (var expense in OtherExpenses)
                {
                    expense.ApprovalStatus = "Approved"; // 'Approved' or 'Rejected'
                    expense.ApprovalDate = DateTime.Now;
                    expense.ApprovedBy = employeeId;
                }
                await _context.SaveChangesAsync();

                // Commit the transaction if everything succeeds
                await transaction.CommitAsync();

                return Ok(new { message = "Payment Approved successfully." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }

        //public async Task<ActionResult> UpdateApproval(int RecordID, string ExpenseType, [FromForm] ApprovalViewModel approvalViewModel)
        //{
        //    Console.WriteLine(ExpenseType + "   " + RecordID);

        //    if (string.IsNullOrEmpty(ExpenseType))
        //    {
        //        approvalViewModel.ErrorMessage = "Expense Type is required.";
        //        return RedirectToAction("Details", new { RecordID, ExpenseType });
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        var employeeId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //        try
        //        {
        //            if (ExpenseType == "Travel")
        //            {
        //                Console.WriteLine(ExpenseType + "   " + RecordID);
        //                var expenseDetails = await _context.TravelExpenses.FirstOrDefaultAsync(te => te.RecordID == RecordID);

        //                if (expenseDetails == null)
        //                {
        //                    approvalViewModel.ErrorMessage = "Travel expense not found.";
        //                    return RedirectToAction("Details", new { RecordID, ExpenseType });
        //                }

        //                expenseDetails.ApprovalStatus = approvalViewModel.ApprovalStatus;
        //                expenseDetails.ApprovalDate = DateTime.Now;
        //                expenseDetails.ApprovedBy = employeeId;
        //                expenseDetails.DeductionAmount = approvalViewModel.DeductionAmount;
        //                expenseDetails.DeductionRemark = approvalViewModel.DeductionRemark;

        //                _context.TravelExpenses.Update(expenseDetails);
        //                await _context.SaveChangesAsync();

        //                return View("EmployeesExpenseList");
        //            }
        //            else
        //            {
        //                Console.WriteLine(ExpenseType + "   " + RecordID);
        //                var expenseDetails = await _context.OtherExpenses.FirstOrDefaultAsync(te => te.RecordID == RecordID);

        //                if (expenseDetails == null)
        //                {
        //                    approvalViewModel.ErrorMessage = "Expense not found.";
        //                    return RedirectToAction("Details", new { RecordID, ExpenseType });
        //                }

        //                expenseDetails.ApprovalStatus = approvalViewModel.ApprovalStatus;
        //                expenseDetails.ApprovalDate = DateTime.Now;
        //                expenseDetails.ApprovedBy = employeeId;
        //                expenseDetails.DeductionAmount = approvalViewModel.DeductionAmount;
        //                expenseDetails.DeductionRemark = approvalViewModel.DeductionRemark;

        //                _context.OtherExpenses.Update(expenseDetails);
        //                await _context.SaveChangesAsync();

        //                return View("EmployeesExpenseList");
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            approvalViewModel.ErrorMessage = $"An error occurred: {ex.Message}";
        //            return RedirectToAction("Details", new { RecordID, ExpenseType });
        //        }
        //    }
        //    else
        //    {
        //        approvalViewModel.ErrorMessage = "Invalid input. Please correct the errors and try again.";
        //        return RedirectToAction("Details", new { RecordID, ExpenseType });
        //    }
        //}

        [PermissionAuthorize("ReadEmpAttendance")]
        public ActionResult UpdateDailyEmployeeEntry()
        {
            var DailyEmployeeEntries = _context.DailyEmployeeEntry
                .Where(e => e.TimeIn.Value.Date == DateTime.Today)
                .ToList();
            return View(DailyEmployeeEntries);
        }

        public async Task<IActionResult> GetAllAbsentEmployees()
        {
            var today = DateTime.Today;

            var employeesWithoutEntries = await _context.EmployeeMaster
                .Where(em => !_context.DailyEmployeeEntry
                    .Any(de => de.EmployeeID == em.EmployeeID && de.TimeIn != null && de.TimeIn.Value.Date == today))
                .ToListAsync();

            return Json(employeesWithoutEntries);
        }

        [PermissionAuthorize("UpdateEmpAttendance")]
        public async Task<IActionResult> EnterEmployeeEntry(DailyEmployeeEntry dailyEmployeeEntry)
        {
            // Log the data for debugging purposes
            Console.WriteLine(dailyEmployeeEntry.TimeIn + dailyEmployeeEntry.InEntryType + "InTime");
            Console.WriteLine(dailyEmployeeEntry.TimeOut + dailyEmployeeEntry.OutEntryType + "OutTime");

            if (dailyEmployeeEntry.EmployeeName == null)
            {
                TempData["ErrorMessage"] = $"Employee Name is Required";
                return RedirectToAction("UpdateDailyEmployeeEntry");
            }
            if (dailyEmployeeEntry.InEntryType == null && dailyEmployeeEntry.OutEntryType == null)
            {
                TempData["ErrorMessage"] = $"Attendance Type is Required";
                return RedirectToAction("UpdateDailyEmployeeEntry");
            }
            // Set the logged status
            if (dailyEmployeeEntry.OutEntryType != null)
            {
                var employeeId = dailyEmployeeEntry.EmployeeID;
                var today = DateTime.Today;

                var isLoggedIn = await _context.DailyEmployeeEntry
                    .Where(e => e.EmployeeID == employeeId && EF.Functions.DateDiffDay(e.TimeIn, today) == 0)
                    .FirstOrDefaultAsync();

                if (isLoggedIn != null)
                {
                    isLoggedIn.TimeOut = dailyEmployeeEntry.TimeOut ?? DateTime.Now;

                    var duration = isLoggedIn.TimeOut.Value - isLoggedIn.TimeIn;
                    TimeSpan timeSpan = TimeSpan.Parse(duration.ToString());
                    var durationString = duration.ToString();

                    Console.WriteLine(timeSpan.Hours);
                    Console.WriteLine(timeSpan.Minutes);
                    isLoggedIn.LoggedStatus = "LoggedOut";
                    isLoggedIn.OutEntryType = dailyEmployeeEntry.OutEntryType;
                    isLoggedIn.WorkingDuration = $"{timeSpan.Hours}:{timeSpan.Minutes}:{timeSpan.Seconds}";
                    // Save changes to the database
                    await _context.SaveChangesAsync();

                    return RedirectToAction("UpdateDailyEmployeeEntry");
                }
                else
                {
                    TempData["ErrorMessage"] = $"{dailyEmployeeEntry.EmployeeName} is not Logged In";
                }
            }
            else
            {
                dailyEmployeeEntry.TimeIn = dailyEmployeeEntry.TimeIn ?? DateTime.Now;
                dailyEmployeeEntry.LoggedStatus = "LoggedIn";
                Console.WriteLine(dailyEmployeeEntry.EmployeeName + "   " + dailyEmployeeEntry.EmployeeID + "   " + dailyEmployeeEntry.LoggedStatus + "     " + dailyEmployeeEntry.TimeIn);
                if (ModelState.IsValid)
                {
                    // Add the entry to the database
                    _context.DailyEmployeeEntry.Add(dailyEmployeeEntry);
                    await _context.SaveChangesAsync();

                    // Redirect to the same page (or another page) to reload
                    //TempData["SuccessMessage"] = "Entry successfully saved";
                    return RedirectToAction("UpdateDailyEmployeeEntry");
                }

                TempData["ErrorMessage"] = "Something went wrong";
                return RedirectToAction("UpdateDailyEmployeeEntry");
            }

            //TempData["ErrorMessage"] = "Something went wrong";
            return RedirectToAction("UpdateDailyEmployeeEntry");
        }

        public async Task<IActionResult> GetEmployeeInEntry(string employeeID)
        {
            var today = DateTime.Now;
            var employeeEntryData = await _context.DailyEmployeeEntry
                .Where(e => e.EmployeeID == employeeID && EF.Functions.DateDiffDay(e.TimeIn, today) == 0)
                .Select(e => e.InEntryType)
                .FirstOrDefaultAsync();
            Console.WriteLine(employeeEntryData + "employeeEntryData");

            var OutEntryType = await _context.AttendanceEntryType
                .Where(e => e.InEntryText == employeeEntryData)
                .Select(e => e.OutEntryText)
                .FirstOrDefaultAsync();

            return Json(OutEntryType);
        }
    }
}

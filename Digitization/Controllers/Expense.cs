using System.Security.Claims;
using Digitization.Models;
using Digitization.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Digitization.Controllers
{
    //[Authorize(Policy = "SharedAccess")]
    public class Expense : Controller
    {
        private readonly ApplicationDBContext _context;

        public Expense(ApplicationDBContext context)
        {
            _context = context;
        }
        // GET: Expense
        [PermissionAuthorize("ReadMaterialCost")]
        public async Task<IActionResult> Index()
        {
            //var ProjectMaterialExpenses = await _context.ProjectMaterialExpenses.ToListAsync();
            return View();
        }

        [PermissionAuthorize("ReadMaterialCost")]
        public async Task<IActionResult> AllMaterialExpenses()
        {
            var ProjectMaterialExpenses = await _context.ProjectMaterialExpenses
                .Where(m => m.IsDeleted == null || m.IsDeleted == false)
                .ToListAsync();

            return Json(ProjectMaterialExpenses);
        }

        // GET: Expense/Create
        [PermissionAuthorize("CreateMaterialCost")]
        [HttpPost]
        public async Task<IActionResult> CreateMaterialExpense(ProjectMaterialExpenses materialExpense)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Corrected raw SQL query
                    //    await _context.Database.ExecuteSqlRawAsync(
                    //        @"INSERT INTO ProjectMaterialExpenses 
                    //(ProjectID, ProjectName, Ref, Remark, MaterialCost, Description)
                    //VALUES ({0}, {1}, {2}, {3}, {4}, {5})",
                    //        materialExpense.ProjectID,
                    //        materialExpense.ProjectName,
                    //        materialExpense.Ref,
                    //        materialExpense.Remark,
                    //        materialExpense.MaterialCost,
                    //        materialExpense.Description
                    //    );
                    Console.WriteLine($"materialExpense.MaterialCost {materialExpense.MaterialCost}");

                    // Add the entity to the DbContext
                    await _context.ProjectMaterialExpenses.AddAsync(materialExpense);
                    await _context.SaveChangesAsync(); // Save changes to the database

                    return RedirectToAction("Index", "Expense");
                }
                catch (Exception ex)
                {
                    // Handle errors
                    ModelState.AddModelError("", "An error occurred while saving the data: " + ex.Message);
                }
            }

            return View("Index");
        }

        [PermissionAuthorize("ReadMaterialCost", "DeleteMaterialCost")]
        [HttpDelete("/Expense/DeleteMaterialExpense/{recordID}")]
        public async Task<IActionResult> DeleteMaterialExpense(int recordID)
        {
            Console.WriteLine($"recordID {recordID}");
            // Find the record by its ID
            var expense = await _context.ProjectMaterialExpenses
                .FirstOrDefaultAsync(m => m.RecordID == recordID && (m.IsDeleted == null || m.IsDeleted == false));

            if (expense == null)
            {
                Console.WriteLine("Record not found or already deleted.");
                // Record not found or already deleted
                return Json(new { success = false, message = "Record not found or already deleted." });
            }

            // Set IsDeleted to true to mark as deleted
            Console.WriteLine("Record marked as deleted.1");
            expense.IsDeleted = true;

            // Save the changes to the database
            await _context.SaveChangesAsync();
            Console.WriteLine("Record marked as deleted.2");
            return Json(new { success = true, message = "Record marked as deleted." });
        }

        public IActionResult SiteExpense()
        {
            return View();
        }

        // POST: Expense/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SiteExpense(SiteExpenses siteExpenses)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Corrected raw SQL query
                    await _context.Database.ExecuteSqlRawAsync(
                        @"INSERT INTO SiteExpenses 
                (ProjectID, SiteLocation, ExpenseType, SiteAmount, ExpenseDescription)
                VALUES ({0}, {1}, {2}, {3}, {4})",
                        siteExpenses.ProjectId,
                        siteExpenses.SiteLocation,
                        siteExpenses.ExpenseType,
                        siteExpenses.SiteAmount,
                        siteExpenses.ExpenseDescription
                    );

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    // Handle errors
                    ModelState.AddModelError("", "An error occurred while saving the data: " + ex.Message);
                }
            }

            return RedirectToAction(nameof(SiteExpense));
        }

        public IActionResult CreateTravelExpenses()
        {
            return View();
        }

        [PermissionAuthorize("ReadTADA")]
        public async Task<IActionResult> AllTravelExpenses()
        {
            var employeeId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var TravelExpenses = await _context.TravelExpenses
                .Where(t => t.EmployeeID == employeeId)
                .ToListAsync();

            return Json(TravelExpenses);
        }

        [HttpPost]
        [PermissionAuthorize("CreateTADA")]
        public async Task<IActionResult> CreateTravelExpenses (TravelExpenses travelExpenses)
        {
            var employeeId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            Console.WriteLine(employeeId);
            if (ModelState.IsValid)
            {
                try
                {
                    travelExpenses.EmployeeID = employeeId;
                    _context.Add(travelExpenses);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("CreateTravelExpenses", "Expense");
                }
                catch(Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while saving the data: " + ex.Message);
                }
            }

            return View(travelExpenses);
        }

        [PermissionAuthorize("ReadTADA(MISC)")]
        public IActionResult CreateOtherExpenses()
        {
            return View();
        }

        [PermissionAuthorize("ReadTADA(MISC)")]
        public async Task<IActionResult> AllOtherExpenses()
        {
            var employeeId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var OtherExpenses = await _context.OtherExpenses
                .Where(t => t.EmployeeID == employeeId)
                .ToListAsync();

            return Json(OtherExpenses);
        }

        [HttpPost]
        [PermissionAuthorize("CreateTADA(MISC)")]
        public async Task<IActionResult> CreateOtherExpenses(OtherExpenses otherExpenses)
        {
            var employeeId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (ModelState.IsValid)
            {
                try
                {
                    otherExpenses.EmployeeID = employeeId;
                    _context.Add(otherExpenses);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("CreateOtherExpenses", "Expense");
                } 
                catch(Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while saving the data: " + ex.Message);
                }
            }

            return View();
        }
    }
}

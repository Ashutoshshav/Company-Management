using Digitization.Models;
using Digitization.Services;
using Digitization.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Digitization.Controllers
{
    //[Authorize(Policy = "DirectorAccess")]
    public class Project : Controller
    {
        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _configuration;

        public Project(ApplicationDBContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [PermissionAuthorize("ReadProjectsData")]
        public async Task<IActionResult> Index()
        {
            //var projectMasters = await (from project in _context.ProjectMaster
            //                            join other in _context.OtherExpenses on project.ProjectId equals other.ProjectID into otherExpenses
            //                            from oe in otherExpenses.DefaultIfEmpty()
            //                            join material in _context.ProjectMaterialExpenses on project.ProjectId equals material.ProjectID into materialExpenses
            //                            from me in materialExpenses.DefaultIfEmpty()
            //                            join travel in _context.TravelExpenses on project.ProjectId equals travel.ProjectID into travelExpenses
            //                            from te in travelExpenses.DefaultIfEmpty()
            //                            group new { project, oe, me, te } by new
            //                            {
            //                                project.ProjectId,
            //                                project.ProjectName,
            //                                project.CustomerName,
            //                                project.EntryDTime
            //                            } into g
            //                            select new ProjectMasterViewModel
            //                            {
            //                                ProjectId = g.Key.ProjectId,
            //                                ProjectName = g.Key.ProjectName,
            //                                CustomerName = g.Key.CustomerName,
            //                                EntryDTime = g.Key.EntryDTime,
            //                                TotalExpense = g.Sum(x =>
            //                                    (x.oe.Amount ?? 0) +
            //                                    (x.me.MaterialCost ?? 0) +
            //                                    (x.te.Amount ?? 0)
            //                                ),
            //                                ExpectedExpense = 43532,
            //                            }).ToListAsync();

            var projectMasters = await _context.ProjectMaster
    .Select(project => new ProjectMasterViewModel
    {
        ProjectId = project.ProjectId,
        ProjectName = project.ProjectName,
        CustomerName = project.CustomerName,
        EntryDTime = project.EntryDTime,

        // Convert nullable float (double?) to double using ?? 0
        MaterialExpenses = _context.ProjectMaterialExpenses
            .Where(pme => pme.ProjectID == project.ProjectId)
            .Sum(pme => (double?)pme.MaterialCost) ?? 0,

        OtherExpenses = _context.OtherExpenses
            .Where(oe => oe.ProjectID == project.ProjectId)
            .Sum(oe => (double?)oe.Amount) ?? 0,

        TravelExpenses = _context.TravelExpenses
            .Where(te => te.ProjectID == project.ProjectId)
            .Sum(te => (double?)te.Amount) ?? 0,

        // Compute total expenses
        TotalExpense = (
            (_context.ProjectMaterialExpenses
                .Where(pme => pme.ProjectID == project.ProjectId)
                .Sum(pme => (double?)pme.MaterialCost) ?? 0)
            +
            (_context.OtherExpenses
                .Where(oe => oe.ProjectID == project.ProjectId)
                .Sum(oe => (double?)oe.Amount) ?? 0)
            +
            (_context.TravelExpenses
                .Where(te => te.ProjectID == project.ProjectId)
                .Sum(te => (double?)te.Amount) ?? 0)
        ),

        ExpectedExpense = 43532 // Static value, change if dynamic
    })
    .ToListAsync();

            //var projectMasters = await (from project in _context.ProjectMaster
            //                            join other in _context.OtherExpenses on project.ProjectId equals other.ProjectID into otherExpenses
            //                            from oe in otherExpenses.DefaultIfEmpty()
            //                            join material in _context.ProjectMaterialExpenses on project.ProjectId equals material.ProjectID into materialExpenses
            //                            from me in materialExpenses.DefaultIfEmpty()
            //                            join travel in _context.TravelExpenses on project.ProjectId equals travel.ProjectID into travelExpenses
            //                            from te in travelExpenses.DefaultIfEmpty()
            //                            group new { project, oe, me, te } by project.ProjectId into g
            //                            select new ProjectMasterViewModel
            //                            {
            //                                ProjectId = g.Key,
            //                                ProjectName = g.FirstOrDefault().project.ProjectName,
            //                                CustomerName = g.FirstOrDefault().project.CustomerName,
            //                                EntryDTime = g.FirstOrDefault().project.EntryDTime,
            //                                TotalExpense = g.Sum(x => (x.oe.Amount ?? 0) +
            //                                                       (x.me.MaterialCost ?? 0) +
            //                                                       (x.te.Amount ?? 0)),
            //                                ExpectedExpense = 43532,
            //                            }).ToListAsync();
            foreach (var item in projectMasters)
            {
                Console.WriteLine(item.TotalExpense);
            }
            return View("Index", projectMasters);
        }

        public async Task<IActionResult> MaterialExpenses()
        {
            var MaterialExpenses = await _context.ProjectMaterialExpenses.ToListAsync();

            return View("MaterialExpenses", MaterialExpenses);
        }

        public IActionResult Create()
        {
            return View("Create");
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProjectMaster projectMaster)
        {
            // Check if ProjectId already exists in the database
            var isProjectIdExists = await _context.ProjectMaster
                                                   .FirstOrDefaultAsync(p => p.ProjectId == projectMaster.ProjectId);
            if (isProjectIdExists != null)
            {
                Console.WriteLine($"{isProjectIdExists}");
                // Add a custom error to the ModelState
                TempData["ErrorMessage"] = $"Project ID {isProjectIdExists.ProjectId} is already exists and is associated with Project Name {isProjectIdExists.ProjectName}";
                return RedirectToAction(nameof(Index));
            }

            // Check if the model is valid
            if (projectMaster.ProjectName != null && projectMaster.ProjectId != null)
            {
                _context.Add(projectMaster);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Project Added Successfully";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ErrorMessage"] = "ProjectID and ProjectName is required";
            }

            // If validation fails, return the view with the validation errors
            return RedirectToAction(nameof(Index));
        }
    }
}

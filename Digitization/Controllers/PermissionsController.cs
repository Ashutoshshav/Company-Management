using Digitization.Models;
using Digitization.Services;
using Digitization.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Digitization.Controllers
{
    public class PermissionsController : Controller
    {
        private readonly ApplicationDBContext _context;

        public PermissionsController(ApplicationDBContext context)
        {
            _context = context;
        }

        [PermissionAuthorize("MngUserAuthorize")]
        public async Task<IActionResult> ManagePermissions()
        {
            var employees = await _context.EmployeeMaster.ToListAsync();

            var allPermissions = await _context.Permissions.ToListAsync();

            var userPermissions = await _context.UserPermissions
                .Include(up => up.Permissions)
                .ToListAsync();

            var allPermissionsDict = await _context.Permissions
                .ToDictionaryAsync(p => p.PermissionsName, p => new { p.PermissionID, p.Description });

            var viewModel = employees.Select(emp => new UserPermissionViewModel
            {
                EmployeeID = emp.EmployeeID,
                EmployeeName = emp.EmployeeName,
                Permissions = allPermissionsDict.ToDictionary(
                    perm => perm.Key,
                    perm => new PermissionDetail
                    {
                        PermissionID = perm.Value.PermissionID,
                        HasPermission = userPermissions.Any(up => up.EmployeeID == emp.EmployeeID && up.PermissionID == perm.Value.PermissionID),
                        Description = perm.Value.Description
                    }
                )
            }).ToList();

            return View(viewModel);
        }

        [HttpPost]
        [PermissionAuthorize("MngUserAuthorize")]
        public async Task<IActionResult> UpdatePermission([FromBody] Dictionary<string, object> data)
        {
            if (data == null || !data.ContainsKey("EmployeeID") || !data.ContainsKey("PermissionID") || !data.ContainsKey("HasPermission"))
                return BadRequest("Invalid data.");

            var employeeID = data["EmployeeID"].ToString();
            var permissionID = data["PermissionID"].ToString();
            var hasPermission = data["HasPermission"].ToString();

            Console.WriteLine($"{employeeID} employeeID {permissionID} permissionID {hasPermission} hasPermission");
            var employeeDetails = await _context.EmployeeMaster.FirstOrDefaultAsync(e => e.EmployeeID == employeeID);

            //Console.WriteLine($"{employeeID} employeeID {permissionID} permissionID {hasPermission} hasPermission");
            
            if (hasPermission == "True")
            {
                string insertQuery = @"
                    INSERT INTO UserPermissions (EmployeeID, PermissionID)
                    VALUES (@p0, @p1)";

                await _context.Database.ExecuteSqlRawAsync(insertQuery, employeeID, permissionID);
                return Ok(new { message = "Permission inserted successfully." });
            }
            else
            {
                string deleteQuery = @"
                    DELETE FROM UserPermissions 
                    WHERE EmployeeID = @p0 AND PermissionID = @p1";

                await _context.Database.ExecuteSqlRawAsync(deleteQuery, employeeID, permissionID);
                return Ok(new { message = "Permission Deleted successfully." });
            }
        }
    }
}

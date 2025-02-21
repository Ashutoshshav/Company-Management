using Digitization.Models;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Digitization.Services;
using Azure.Core;

namespace Digitization.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDBContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            return View("Index");
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(EmployeeMaster Employee)
        {
            Console.WriteLine(Employee.EmployeeEmail + "   " + Employee.EmployeePassword);

            // Find employee by email
            var user = await _context.EmployeeMaster
                .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permissions)
                .FirstOrDefaultAsync(u => u.EmployeeEmail == Employee.EmployeeEmail);

            // Check if user exists and password matches
            if (user != null)
            {
                if (user.EmployeePassword == Employee.EmployeePassword)
                {
                    // Generate JWT token
                    var token = GenerateJwtToken(user);

                    // Store token in HTTP-only cookies (for security)
                    Response.Cookies.Append("Token", token, new CookieOptions
                    {
                        SameSite = SameSiteMode.Strict,
                        Secure = false, // Set to 'true' if using HTTPS
                        Expires = DateTime.UtcNow.AddDays(1) // Token expires in 1 day
                    });

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid Password";
                    return RedirectToAction("Login");
                }
            }
            else
            {
                TempData["ErrorMessage"] = "User doesn't exist";
                return RedirectToAction("Login");
            }
        }

        private string GenerateJwtToken(EmployeeMaster user)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.EmployeeID), // EmployeeID is already a string
                new Claim("Role", user.EmployeeRole)
            };

            // Add permissions as claims
            foreach (var userPermission in user.UserPermissions)
            {
                claims.Add(new Claim("Permit", userPermission.Permissions.PermissionsName));
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1), // Token valid for 1 Day
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

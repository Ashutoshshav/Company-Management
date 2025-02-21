using Digitization.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

public class PermissionAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string[] _requiredPermissions;

    public PermissionAuthorizeAttribute(params string[] requiredPermissions)
    {
        _requiredPermissions = requiredPermissions;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        // 🔹 Ensure the user is authenticated
        if (!user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // 🔹 Extract EmployeeID from JWT claims
        var employeeId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(employeeId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // 🔹 Get the database context
        var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDBContext>();

        // 🔹 Fetch user permissions from the database (Always Live Checking)
        var userPermissions = await (
            from up in dbContext.UserPermissions
            join p in dbContext.Permissions on up.PermissionID equals p.PermissionID
            where up.EmployeeID == employeeId
            select p.PermissionsName
        ).ToListAsync();

        // 🔹 Check if user has all required permissions
        if (!_requiredPermissions.All(permission => userPermissions.Contains(permission)))
        {
            context.Result = new ForbidResult();
        }
    }
}

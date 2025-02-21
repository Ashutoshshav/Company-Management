using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;

namespace Digitization.ViewModel
{
    public class UserPermissionViewModel
    {
        [Key]
        public string EmployeeID { get; set; }
        public string? EmployeeName { get; set; }
        public string? PermissionID { get; set; }

        [NotMapped]
        [BindProperty(Name = "Permissions")]
        public Dictionary<string, PermissionDetail>? Permissions { get; set; }
    }

    // New class to store permission details
    public class PermissionDetail
    {
        public string PermissionID { get; set; }
        public bool HasPermission { get; set; }
        public string? Description { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Digitization.Models
{
    public class Permissions
    {
        [Key]
        public string PermissionID { get; set; }
        public string PermissionsName { get; set; }
        public string? Description { get; set; }
        public ICollection<UserPermissions> UserPermissions { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digitization.Models
{
    public class UserPermissions
    {
        [Key]
        public int RecordID { get; set; }

        [ForeignKey("EmployeeMaster")]
        public string EmployeeID { get; set; }

        public EmployeeMaster EmployeeMaster { get; set; }

        [ForeignKey("Permissions")]
        public string PermissionID { get; set; }  

        public Permissions Permissions { get; set; }
    }
}

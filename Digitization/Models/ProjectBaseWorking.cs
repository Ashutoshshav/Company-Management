using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digitization.Models
{
    public class ProjectBaseWorking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Ensure it's auto-generated
        public int RecordID { get; set; }

        [Required(ErrorMessage = "Project ID is required.")]
        [StringLength(50, ErrorMessage = "Project ID cannot exceed 50 characters.")]
        public string ProjectID { get; set; }
        
        public string EmployeeID { get; set; }

        [StringLength(50, ErrorMessage = "Description cannot exceed 100 characters.")]
        public string? Description { get; set; }

        public DateTime StartDTime { get; set; }

        public DateTime? EndDTime { get; set; }

        public string WorkDuration { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Ensure it's auto-generated
        public DateTime? EntryDTime { get; set; }

        //[StringLength(50, ErrorMessage = "Remark cannot exceed 100 characters.")]
        //public string? Remark { get; set; }
    }
}

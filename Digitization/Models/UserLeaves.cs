using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digitization.Models
{
    public class UserLeaves
    {
        [Key]
        public int RecordID { get; set; }

        [Required(ErrorMessage = "User ID is required.")]
        public string UserID { get; set; }

        [Required(ErrorMessage = "Reason is required.")]
        public string Reason { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Leave Starting Date is required.")]
        public DateTime LeaveStart { get; set; }

        [Required(ErrorMessage = "Leave Ending Date is required.")]
        public DateTime LeaveEnd { get; set; }

        public string? Status { get; set; }

        public string? ApprovedBy { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime? CreatedAt { get; set; }
    }
}

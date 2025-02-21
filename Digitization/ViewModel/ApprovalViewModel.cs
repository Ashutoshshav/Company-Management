using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Digitization.ViewModel
{
    public class ApprovalViewModel
    {
        [Key]
        public int RecordID { get; set; }

        public string ExpenseType { get; set; } // "Travel" or "Other"

        //[Required(ErrorMessage = "ApprovalStatus is required.")]
        [StringLength(50, ErrorMessage = "ApprovalStatus cannot exceed 50 characters.")]
        public string ApprovalStatus { get; set; }

        public string? ApprovedBy { get; set; }

        public DateTime? ApprovalDate { get; set; }

        public double? DeductionAmount { get; set; }

        public string DeductionRemark { get; set; }

        public string ErrorMessage { get; set; }
    }
}

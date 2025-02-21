using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Digitization.Models
{
    public class TravelExpenses
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Ensure it's auto-generated
        public int RecordID { get; set; }

        [Required(ErrorMessage = "Project ID is required.")]
        [StringLength(50, ErrorMessage = "Project ID cannot exceed 50 characters.")]
        public string? ProjectID { get; set; }

        //[Required(ErrorMessage = "Employee ID is required.")]
        //[StringLength(50, ErrorMessage = "Employee ID cannot exceed 50 characters.")]
        public string? EmployeeID { get; set; }

        [Required(ErrorMessage = "TravelBy is required.")]
        [StringLength(50, ErrorMessage = "TravelBy cannot exceed 50 characters.")]
        public string? TravelBy { get; set; }

        [Required(ErrorMessage = "TransportMode is required.")]
        [StringLength(50, ErrorMessage = "TransportMode cannot exceed 50 characters.")]
        public string? TransportMode { get; set; }

        [Required(ErrorMessage = "FromPlace is required.")]
        [StringLength(50, ErrorMessage = "FromPlace cannot exceed 50 characters.")]
        public string? FromPlace { get; set; }

        [Required(ErrorMessage = "ToPlace is required.")]
        [StringLength(50, ErrorMessage = "ToPlace cannot exceed 50 characters.")]
        public string? ToPlace { get; set; }

        //[Required (ErrorMessage = "Description is required.")]
        [StringLength(100, ErrorMessage = "Description cannot exceed 100 characters.")]
        public string? Description { get; set; }

        //[Required(ErrorMessage = "KMs is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "KMs must be a positive value.")]
        public double? KMs { get; set; }

        //[Required(ErrorMessage = "Rate is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Rate must be a positive value.")]
        public double? Rate { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be a positive value.")]
        public double? Amount { get; set; }

        //[Required (ErrorMessage = "Remark is required.")]
        [StringLength(100, ErrorMessage = "Remark cannot exceed 100 characters.")]
        public string? Remark { get; set; }

        //[Required(ErrorMessage = "ApprovalStatus is required.")]
        //[StringLength(50, ErrorMessage = "ApprovalStatus cannot exceed 50 characters.")]
        public string? ApprovalStatus { get; set; }

        [DataType(DataType.Date, ErrorMessage = "date format should be valid.")]
        public DateTime? TravelDate { get; set; }

        //[DataType(DataType.Date, ErrorMessage = "ApprovalDate date format.")]
        public DateTime? ApprovalDate { get; set; }

        public string? ApprovedBy { get; set; }

        //[Required(ErrorMessage = "Deduction Amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Deduction Amount must be a positive value.")]
        public double? DeductionAmount { get; set; }

        //[Required(ErrorMessage = "Deduction Remark is required.")]
        //[StringLength(100, ErrorMessage = "Deduction Remark cannot exceed 100 characters.")]
        public string? DeductionRemark { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Ensure it's auto-generated
        public DateTime? EntryDTime { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Digitization.ViewModel
{
    public class PaymentApprovalViewModel
    {

        [Key]
        public int EntryID { get; set; }
        public int RecordID { get; set; }
        public string? ProjectID { get; set; }
        public string? EmployeeID { get; set; }
        public string? ExpenseType { get; set; }
        public double? Amount { get; set; }
        public string? ApprovalStatus { get; set; }
        public DateTime? ExpenseDate { get; set; }
        public DateTime? EntryDTime { get; set; }
    }
}

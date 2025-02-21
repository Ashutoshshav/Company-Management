using System.ComponentModel.DataAnnotations;

namespace Digitization.ViewModel
{
    public class EmployeeExpenseViewModel
    {
        [Key]
        public int RecordID { get; set; }
        public string? EmployeeID { get; set; }
        public string? EmployeeName { get; set; }
        public double? TotalExpense { get; set; }
    }
}

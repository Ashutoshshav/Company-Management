using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Digitization.ViewModel
{
    public class EmployeeExpenseDetailsViewModel
    {
        [Key]
        public int EntryID { get; set; }
        public string EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public List<PaymentApprovalViewModel> PaymentApprovalViewModel { get; set; }
    }
}

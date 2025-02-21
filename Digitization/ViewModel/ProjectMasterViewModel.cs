using System.ComponentModel.DataAnnotations;

namespace Digitization.ViewModel
{
    public class ProjectMasterViewModel
    {
        [Key]
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string CustomerName { get; set; }
        public DateTime? EntryDTime { get; set; }
        //public double? TotalExpense { get; set; }
        //public decimal? ExpectedExpense { get; set; }
        // Change to double since the database uses FLOAT
        public double MaterialExpenses { get; set; }
        public double OtherExpenses { get; set; }
        public double TravelExpenses { get; set; }

        public double TotalExpense { get; set; }
        public double ExpectedExpense { get; set; }
    }
}

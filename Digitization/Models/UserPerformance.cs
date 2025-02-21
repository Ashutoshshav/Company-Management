using System.ComponentModel.DataAnnotations;

namespace Digitization.Models
{
    public class UserPerformance
    {
        [Key]
        public int RecordID { get; set; }

        public string? EmployeeID { get; set; }

        public int? PerformanceID { get; set; }

        public double? PerformanceScore { get; set; }
    }
}

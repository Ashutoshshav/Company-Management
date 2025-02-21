using System.ComponentModel.DataAnnotations;

namespace Digitization.Models
{
    public class PerformanceTypes
    {
        [Key]
        public int PerformanceID { get; set; }

        public string? PerformanceType { get; set; }

        public string? PerformanceDescription { get; set; }
    }
}

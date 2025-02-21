using System.ComponentModel.DataAnnotations;

namespace Digitization.Models
{
    public class GenratedChallan
    {
        [Key]
        public int RecordID { get; set; }
        public int? Challan_ID { get; set; }
        public string? Challan_FY { get; set; }
        public DateTime? DTime { get; set; }
        public int? Challan_Type { get; set; }
        public int? PartyID { get; set; }
        public string? DescriptionOfGoods { get; set; }
        public int? Qty { get; set; }
        public string? Remark { get; set; }


    }
}

using System.ComponentModel.DataAnnotations;

namespace Digitization.Models
{
    public class PORequest
    {
        [Key]
        public int RecordID { get; set; }
        public string PartyName { get; set; }
        public string ContactPerson { get; set; }
        public string Address { get; set; }
        public string MobileNo { get; set; }
        public string SenderName { get; set; }
        public string Subject { get; set; }
        public List<TableData> TableData { get; set; }

    }
    public class TableData
    {
        [Key]
        public string ItemCode { get; set; }
        public string ItemDescription { get; set; }
        public string HsnSac { get; set; }
        public string UnitRate { get; set; }
        public string Quantity { get; set; }
        public string Discount { get; set; }
        public string Amount { get; set; }
        public string GstRate { get; set; }
        public string GstAmount { get; set; }
    }
}

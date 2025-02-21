using System.ComponentModel.DataAnnotations;

namespace Digitization.Models
{
    public class PartyMaster
    {
        [Key]
        public int RecordID { get; set; }
        public DateTime? EntryDTime { get; set; }
        public DateTime? UpdateDTime { get; set; }
        public int? PartyID { get; set; }
        public string? PartyName { get; set; }
        public string? ContactPerson { get; set; }
        public int? PartyStatus { get; set; }
        public string? ContactNo { get; set; }
        public string? GSTNo { get; set; }
        public string? PANNo { get; set; }
        public string? Email { get; set; }
        public string? Department { get; set; }
        public string? Bank { get; set; }
        public string? Address { get; set; }
        public string? Remark { get; set; }

    }
}

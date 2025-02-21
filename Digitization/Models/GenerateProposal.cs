using System.ComponentModel.DataAnnotations;

namespace Digitization.Models
{
    public class GenerateProposal
    {
        [Key]
        public int RecordID { get; set; }
        public DateTime? DTime { get; set; }
        public int? PartyID { get; set; }
        public int? Proposal_ID { get; set; }
        public string? Subject { get; set; }
        public string? ItemDescription { get; set; }
        public string? HSN_SAC_NO { get; set; }
        public int? Unit_Rate { get; set; }
        public int? QTY { get; set; }
        public int? Amount { get; set; }
        public string? DeliveryTime { get; set; }
        public string? Proposal_FY { get; set; }
        public string? SenderName { get; set; }
    }
}

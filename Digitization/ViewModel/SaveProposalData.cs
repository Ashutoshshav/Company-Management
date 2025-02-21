using static Digitization.Controllers.Challan;

namespace Digitization.ViewModel
{
    public class SaveProposalData
    {

       // public string PartyName { get; set; }
        public string? PartyName { get; set; }
        public string? Person { get; set; }
        public string? SenderName { get; set; }
        public string? Subject { get; set; }
        public string? Proposal_FY { get; set; }
        public int? PartyID { get; set; }
        public int? Proposal_ID { get; set; }
        public DateTime? DTime { get; set; }


        public List<SaveProposalData>? Items { get; set; }
        public string? ItemDescription { get; set; }
        public string? HSN_SAC_NO { get; set; }
        public int? Unit_Rate { get; set; }
        public int? Qty { get; set; }
        public int? Amount { get; set; }
        public string? DeliveryTime { get; set; }
    }
}

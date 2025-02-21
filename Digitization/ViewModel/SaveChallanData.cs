using static Digitization.Controllers.Challan;

namespace Digitization.ViewModel
{
    public class SaveChallanData
    {

        public string PartyName { get; set; }
        public int PartyID { get; set; }
        public string Challan_Type { get; set; }
        public string CurrentDate { get; set; }
        public List<SaveChallanData> Items { get; set; }
        public string Description { get; set; }
        public string Qty { get; set; }
        public string Remark { get; set; }
    }
}

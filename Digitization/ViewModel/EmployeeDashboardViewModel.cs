namespace Digitization.ViewModel
{
    public class EmployeeDashboardViewModel
    {
        public AttendanceChartViewModel AttendanceData { get; set; }
        //public TaskChartViewModel TaskData { get; set; }
        //public PerformanceChartViewModel PerformanceData { get; set; }
        //public VoucherChartViewModel VoucherData { get; set; }
    }

    public class AttendanceChartViewModel
    {
        public string JobCategory { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public double Percentage { get; set; }
        public List<EmployeeAttendanceDetails> AttendanceDetails { get; set; }
    }

    public class EmployeeAttendanceDetails
    {
        public string EmployeeName { get; set; }
        public string JobCategory { get; set; }
        public string TimeIn { get; set; }
    }
}

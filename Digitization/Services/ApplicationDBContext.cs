using Digitization.Models;
using Digitization.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace Digitization.Services
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options) { }

        public virtual DbSet<AttendanceEntryType> AttendanceEntryType { get; set; }

        public virtual DbSet<AdminMaster> AdminMaster { get; set; }

        public virtual DbSet<DailyEmployeeEntry> DailyEmployeeEntry { get; set; }

        public virtual DbSet<EmployeeMaster> EmployeeMaster { get; set; }

        public virtual DbSet<ProjectMaterialExpenses> ProjectMaterialExpenses { get; set; }

        public virtual DbSet<MeetingMaster> MeetingMaster { get; set; }

        public virtual DbSet<ProjectMaster> ProjectMaster { get; set; }

        public virtual DbSet<TravelExpenses> TravelExpenses { get; set; }

        public virtual DbSet<OtherExpenses> OtherExpenses { get; set; }

        public virtual DbSet<ProjectBaseWorking> ProjectBaseWorking { get; set; }

        public virtual DbSet<ProjectExpenseVoucher> ProjectExpenseVoucher { get; set; }

        public virtual DbSet<SiteExpenses> SiteExpenses { get; set; }

        public virtual DbSet<SiteVisitHistory> SiteVisitHistory { get; set; }

        public virtual DbSet<WorkMaster> WorkMaster { get; set; }

        public virtual DbSet<ProjectMasterViewModel> ProjectMasterViewModel { get; set; }

        public virtual DbSet<PaymentApprovalViewModel> PaymentApprovalViewModel { get; set; }

        public virtual DbSet<EmployeeExpenseViewModel> EmployeeExpenseViewModel { get; set; }

        public virtual DbSet<EmployeeExpenseDetailsViewModel> EmployeeExpenseDetailsViewModel { get; set; }

        public virtual DbSet<UserPermissions> UserPermissions { get; set; }

        public virtual DbSet<Permissions> Permissions { get; set; }

        public virtual DbSet<UserPermissionViewModel> UserPermissionViewModel { get; set; }

        public virtual DbSet<PartyMaster> PartyMaster { get; set; }

        public virtual DbSet<GenratedChallan> GenratedChallan { get; set; }

        public virtual DbSet<GenerateProposal> GenerateProposal { get; set; }

        public virtual DbSet<PerformanceTypes> PerformanceTypes { get; set; }

        public virtual DbSet<UserPerformance> UserPerformance { get; set; }

        public virtual DbSet<UserLeaves> UserLeaves { get; set; }

        public virtual DbSet<PORequest> PORequest { get; set; }
    }
}

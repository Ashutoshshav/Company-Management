using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Digitization.Models;

public partial class WorkMaster
{
    [Key]
    public int WorkID { get; set; }

    public string? WorkName { get; set; }

    public string? EmployeeID { get; set; }

    public string? EmployeeName { get; set; }

    public string? EmployeeWork { get; set; }

    public string? AssignedBy { get; set; }

    public DateTime? AssignedDtime { get; set; }

    public int? WorkCategory { get; set; }

    public DateTime? Deadline { get; set; }

    public DateTime? StartDtime { get; set; }

    public DateTime? EndDtime { get; set; }

    public string? CompleatingDuration { get; set; }

    public string? WorkRemarks { get; set; }

    public int? WorkStatus { get; set; }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Digitization.Models;

public partial class MeetingMaster
{
    [Key]
    public int MeetingId { get; set; }

    public DateTimeOffset MeetingDtime { get; set; }

    public string MeetingLocation { get; set; } = null!;

    public string? MeetingDescription { get; set; }

    public int EmployeeId { get; set; }

    public DateTimeOffset AssignedDtime { get; set; }
}

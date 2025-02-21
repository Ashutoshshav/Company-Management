using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Digitization.Models;

public partial class SiteVisitHistory
{
    [Key]
    public int EntryNo { get; set; }

    public int? EmployeeId { get; set; }

    public byte[]? SiteImage { get; set; }

    public double? SiteLatitude { get; set; }

    public double? SiteLongitude { get; set; }

    public DateTimeOffset? EntryDtime { get; set; }

    public string? SiteLocation { get; set; }
}

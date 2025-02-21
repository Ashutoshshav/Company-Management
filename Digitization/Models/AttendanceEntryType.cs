using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Digitization.Models;

public partial class AttendanceEntryType
{
    [Key]
    public int RecordID { get; set; }

    public string InEntryText { get; set; }

    public string? OutEntryText { get; set; }
}

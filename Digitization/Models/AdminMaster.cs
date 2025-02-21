using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Digitization.Models;

public partial class AdminMaster
{
    [Key]
    public int? AdminId { get; set; }

    public string? AdminName { get; set; }

    public string? AdminEmail { get; set; }

    public string? AdminMob { get; set; }

    public string? AdminPassword { get; set; }

    public string? AdminRole { get; set; }
}

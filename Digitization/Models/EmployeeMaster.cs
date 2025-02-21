using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Digitization.Models;

public partial class EmployeeMaster
{
    [Key]
    public string? EmployeeID { get; set; }

    public string? EmployeeName { get; set; }

    public string? EmployeeEmail { get; set; }

    public string? EmployeePassword { get; set; }

    public string? EmployeeRole { get; set; }

    public string? EmployeeMob { get; set; }

    public ICollection<UserPermissions> UserPermissions { get; set; }
}

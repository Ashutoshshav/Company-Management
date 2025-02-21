using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Digitization.Models;

public partial class ProjectExpenseVoucher
{
    [Key]
    public string? ProjectId { get; set; }

    public string? ProjectName { get; set; }

    public DateTime? ExpenseDate { get; set; }

    public string? ExpenseType { get; set; }

    public double? ExpenseAmount { get; set; }
}

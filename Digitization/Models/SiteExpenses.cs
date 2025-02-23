using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digitization.Models;

public partial class SiteExpenses
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Ensure it's auto-generated
    public int RecordID { get; set; }

    [Required (ErrorMessage = "Project ID is required.")]
    public string? ProjectId { get; set; }

    [Required (ErrorMessage = "Location is required.")]
    [StringLength (50, ErrorMessage = "Location cannot exceed 50 characters.")]
    public string? SiteLocation { get; set; }

    [Required (ErrorMessage = "Expense Type is required.")]
    [StringLength (50, ErrorMessage = "Expense Type cannot exceed 50 characters.")]
    public string? ExpenseType { get; set; }

    [Required (ErrorMessage = "Expense Amount is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Expense Amount must be a positive value.")]
    public double? SiteAmount { get; set; }

    [StringLength(100, ErrorMessage = "Expense Description cannot exceed 100 characters.")]
    public string? ExpenseDescription { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime? EntryDTime { get; set; }
    // Navigation property for the related ProjectMaster
    //public ProjectMaster ProjectMaster { get; set; }
}

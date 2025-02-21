using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;

namespace Digitization.Models;

public partial class ProjectMaster
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Ensure it's auto-generated
    public int RecordID { get; set; }

    [Key]
    [Required (ErrorMessage = "Project ID is required.")]
    [StringLength(50, ErrorMessage = "Project ID cannot exceed 50 characters.")]
    public string ProjectId { get; set; }

    [Required (ErrorMessage = "Project Name is required.")]
    [StringLength(50, ErrorMessage = "Project Name cannot exceed 50 characters.")]
    public string? ProjectName { get; set; }
    
    //[Required (ErrorMessage = "Customer Name is required.")]
    [StringLength(50, ErrorMessage = "Customer Name cannot exceed 50 characters.")]
    public string? CustomerName { get; set; }
    
    //[Required (ErrorMessage = "Description is required.")]
    [StringLength(50, ErrorMessage = "Description cannot exceed 100 characters.")]
    public string? Description { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Ensure it's auto-generated
    public DateTime? EntryDTime { get; set; }

    ////[Required(ErrorMessage = "Amount is required.")]
    ////[Range(0.01, double.MaxValue, ErrorMessage = "Amount must be a positive value.")]
    //public decimal? TotalExpense { get; set; }
}

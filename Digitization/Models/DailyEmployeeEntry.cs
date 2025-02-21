using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digitization.Models;

public partial class DailyEmployeeEntry
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int RecordID { get; set; }

    [Required(ErrorMessage = "Employee ID is required.")]
    [StringLength(50, ErrorMessage = "Employee ID cannot exceed 50 characters.")]
    public string EmployeeID { get; set; }

    [Required(ErrorMessage = "Employee Name is required.")]
    [StringLength(50, ErrorMessage = "Employee Name cannot exceed 50 characters.")]
    public string EmployeeName { get; set; }

    //[Required(ErrorMessage = "Time In is required.")]
    public DateTime? TimeIn { get; set; }

    public DateTime? TimeOut { get; set; }

    public string? LoggedStatus { get; set; }

    public string? WorkingDuration { get; set; }

    //[Required(ErrorMessage = "In Entry is required.")]
    public string? InEntryType { get; set; }
    
    //[Required(ErrorMessage = "Out Entry is required.")]
    public string? OutEntryType { get; set; }

    //public DateOnly? Date { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Ensure it's auto-generated
    public DateTime? EntryDTime { get; set; }
}

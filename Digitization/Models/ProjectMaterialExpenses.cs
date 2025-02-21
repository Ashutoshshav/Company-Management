using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digitization.Models;

public partial class ProjectMaterialExpenses
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Ensure it's auto-generated
    public int RecordID { get; set; }

    [Required(ErrorMessage = "Project ID is required.")]
    [StringLength(50, ErrorMessage = "Project ID cannot exceed 50 characters.")]
    public string ProjectID { get; set; }

    //[Required(ErrorMessage = "Project Name is required.")]
    [StringLength(50, ErrorMessage = "Project Name cannot exceed 50 characters.")]
    public string? ProjectName { get; set; }


    [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters.")]
    public string? Description { get; set; }

    [StringLength(50, ErrorMessage = "Reference Number cannot exceed 50 characters.")]
    public string? Ref { get; set; }

    //[Required(ErrorMessage = "Material Amount is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Material Amount must be a positive value.")]
    public double? MaterialCost { get; set; }

    [StringLength(100, ErrorMessage = "Remark cannot exceed 100 characters.")]
    public string? Remark { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Ensure it's auto-generated
    public DateTime? EntryDTime { get; set; }

    public Boolean? IsDeleted { get; set; }

    //[DataType(DataType.Date, ErrorMessage = "date format.")]
    //public DateTime? ReferenceDate { get; set; }

    //[Required(ErrorMessage = "Party Name is required.")]
    //[StringLength(50, ErrorMessage = "Party Name cannot exceed 50 characters.")]
    //public string? PartyName { get; set; }

    // Navigation property for the related ProjectMaster
    //public ProjectMaster ProjectMaster { get; set; }
}

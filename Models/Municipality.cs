using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Municipal_Elections_Management_System.Models;

public class Municipality
{
    public int MunicipalityId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Province { get; set; } = "British Columbia";

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Election Date")]
    public DateTime ElectionDate { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(255)]
    [Display(Name = "Logo")]
    public string? LogoFileName { get; set; }

    // uploaded logo, saved as a file (not a db column)
    [NotMapped]
    [Display(Name = "Logo")]
    public IFormFile? LogoFile { get; set; }

    // Identity role whose members administer this municipality (e.g. "Pitt Meadows Admin").
    // Set by the system when the municipality is created, never from a form.
    [ValidateNever]
    [StringLength(150)]
    [Display(Name = "Admin Group")]
    public string AdminRoleName { get; set; } = string.Empty;

    public ICollection<Position> Positions { get; set; } = new List<Position>();
}

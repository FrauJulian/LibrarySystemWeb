using System.ComponentModel.DataAnnotations;

namespace Library.Web.Models;

public sealed class StudentUpsertViewModel
{
    [Required]
    [StringLength(20)]
    [Display(Name = "Ausweisnummer")]
    public string CardNumber { get; set; } = "";

    [Required]
    [StringLength(100)]
    [Display(Name = "Vorname")]
    public string FirstName { get; set; } = "";

    [Required]
    [StringLength(100)]
    [Display(Name = "Nachname")]
    public string LastName { get; set; } = "";

    [Display(Name = "Aktiv")] public bool IsActive { get; set; } = true;
}
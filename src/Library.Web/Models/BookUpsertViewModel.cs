using System.ComponentModel.DataAnnotations;

namespace Library.Web.Models;

public class BookUpsertViewModel
{
    [Required]
    [StringLength(10)]
    [RegularExpression(@"^\d{5}-\d{4}$", ErrorMessage = "Format: xxxxx-jjjj (z.B. 01234-2024)")]
    [Display(Name = "Buchnummer")]
    public string BookNumber { get; set; } = "";

    [Required]
    [StringLength(200)]
    [Display(Name = "Titel")]
    public string Title { get; set; } = "";

    [Required]
    [StringLength(200)]
    [Display(Name = "Autor/Herausgeber")]
    public string AuthorOrEditor { get; set; } = "";

    [Required]
    [Display(Name = "Sachgebiet")]
    public int SubjectId { get; set; }

    [StringLength(20)]
    [Display(Name = "ISBN")]
    public string? Isbn { get; set; }

    [StringLength(200)]
    [Display(Name = "Verlag")]
    public string? Publisher { get; set; }

    [StringLength(100)]
    [Display(Name = "Verlagsort")]
    public string? PublisherCity { get; set; }

    [Display(Name = "Erscheinungsdatum")]
    [DataType(DataType.Date)]
    public DateOnly? PublishedOn { get; set; }
}
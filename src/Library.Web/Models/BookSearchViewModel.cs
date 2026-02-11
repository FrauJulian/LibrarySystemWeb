using System.ComponentModel.DataAnnotations;

namespace Library.Web.Models;

public sealed class BookSearchViewModel
{
    [Display(Name = "Titel enthält")] public string? TitleContains { get; set; }

    [Display(Name = "Autor/Herausgeber enthält")]
    public string? AuthorContains { get; set; }

    [Display(Name = "Sachgebiet")] public int? SubjectId { get; set; }
}
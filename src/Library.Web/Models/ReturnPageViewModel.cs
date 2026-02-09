using System.ComponentModel.DataAnnotations;
using Library.Domain.Dtos;

namespace Library.Web.Models;

public sealed class ReturnPageViewModel
{
    [Required, Display(Name = "Buchnummer")]
    public string BookNumber { get; set; } = "";

    [Display(Name = "Buch suchen")]
    public string Q { get; set; } = "";

    public IReadOnlyList<BookListItemDto> Results { get; set; } = [];
}
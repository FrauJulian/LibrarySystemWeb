using System.ComponentModel.DataAnnotations;
using Library.Domain.Dtos;

namespace Library.Web.Models;

public sealed class CheckoutPageViewModel
{
    [Required, Display(Name = "Ausweisnummer")]
    public string StudentCardNumber { get; set; } = "";

    [Required, Display(Name = "Buchnummer")]
    public string BookNumber { get; set; } = "";

    [Display(Name = "Schüler suchen")]
    public string StudentQ { get; set; } = "";

    [Display(Name = "Buch suchen")]
    public string BookQ { get; set; } = "";

    public IReadOnlyList<StudentListItemDto> StudentResults { get; set; } = [];
    public IReadOnlyList<BookListItemDto> BookResults { get; set; } = [];
}
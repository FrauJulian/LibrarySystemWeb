using System.ComponentModel.DataAnnotations;
using Library.Domain.Dtos;

namespace Library.Web.Models;

public sealed class CheckoutViewModel
{
    [Required, Display(Name = "Ausweisnummer")]
    public string StudentCardNumber { get; set; } = "";

    [Required, Display(Name = "Buchnummer")]
    public string BookNumber { get; set; } = "";
}
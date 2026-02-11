using System.ComponentModel.DataAnnotations;

namespace Library.Web.Models;

public sealed class CheckoutViewModel
{
    [Required]
    [Display(Name = "Ausweisnummer")]
    public string StudentCardNumber { get; set; } = "";

    [Required]
    [Display(Name = "Buchnummer")]
    public string BookNumber { get; set; } = "";
}
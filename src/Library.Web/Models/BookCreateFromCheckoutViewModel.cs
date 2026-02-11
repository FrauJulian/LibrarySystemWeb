using System.ComponentModel.DataAnnotations;

namespace Library.Web.Models;

public sealed class BookCreateFromCheckoutViewModel : BookUpsertViewModel
{
    [Required] public string StudentCardNumber { get; set; } = "";
}
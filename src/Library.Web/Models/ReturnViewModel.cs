using System.ComponentModel.DataAnnotations;

namespace Library.Web.Models;

public sealed class ReturnViewModel
{
    [Required, Display(Name = "Buchnummer")]
    public string BookNumber { get; set; } = "";
}
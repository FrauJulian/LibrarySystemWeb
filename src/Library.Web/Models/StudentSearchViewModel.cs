using System.ComponentModel.DataAnnotations;

namespace Library.Web.Models;

public sealed class StudentSearchViewModel
{
    [Display(Name = "Name enthält")]
    public string? NameContains { get; set; }
}
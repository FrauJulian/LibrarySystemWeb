using Library.Domain.Dtos;

namespace Library.Web.Models;

public sealed class ReturnVerifyViewModel
{
    public string BookNumber { get; set; } = "";
    public ReturnVerificationDto Verification { get; set; } = new("", "", "", "", default, false, null);
}

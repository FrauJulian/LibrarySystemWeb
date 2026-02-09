using Library.Domain.Dtos;

namespace Library.Web.Models;

public sealed class CheckoutVerifyViewModel
{
    public CheckoutRequestDto Request { get; set; } = new("", "");
    public CheckoutVerificationDto Verification { get; set; } = new("", "", "", "", false, null);
}
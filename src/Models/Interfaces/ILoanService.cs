using Library.Domain.Common;
using Library.Domain.Dtos;

namespace Library.Domain.Interfaces;

public interface ILoanService
{
    Task<IReadOnlyList<LoanListItemDto>> GetActiveLoansAsync(CancellationToken cancellationToken = default);

    Task<Results<CheckoutVerificationDto>> VerifyCheckoutAsync(CheckoutRequestDto request, CancellationToken cancellationToken = default);
    Task<Results> ConfirmCheckoutAsync(CheckoutRequestDto request, CancellationToken cancellationToken = default);

    Task<Results<ReturnVerificationDto>> VerifyReturnAsync(string bookNumber, CancellationToken cancellationToken = default);
    Task<Results> ConfirmReturnAsync(string bookNumber, CancellationToken cancellationToken = default);
}

using Library.Domain.Common;
using Library.Domain.Dtos;

namespace Library.Domain.Interfaces;

public interface ILoanService
{
    Task<IReadOnlyList<LoanListItemDto>> GetActiveLoansAsync(CancellationToken cancellationToken = default);

    Task<Result<CheckoutVerificationDto>> VerifyCheckoutAsync(CheckoutRequestDto request, CancellationToken cancellationToken = default);
    Task<Result> ConfirmCheckoutAsync(CheckoutRequestDto request, CancellationToken cancellationToken = default);

    Task<Result<ReturnVerificationDto>> VerifyReturnAsync(string bookNumber, CancellationToken cancellationToken = default);
    Task<Result> ConfirmReturnAsync(string bookNumber, CancellationToken cancellationToken = default);
}

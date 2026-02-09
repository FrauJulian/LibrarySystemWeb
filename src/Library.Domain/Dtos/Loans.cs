namespace Library.Domain.Dtos;

public sealed record LoanListItemDto(
    int LoanId,
    string BookNumber,
    string BookTitle,
    string StudentCardNumber,
    string StudentFullName,
    DateTime LoanedAtUtc
);

public sealed record CheckoutRequestDto(string StudentCardNumber, string BookNumber);

public sealed record CheckoutVerificationDto(
    string StudentCardNumber,
    string StudentFullName,
    string BookNumber,
    string BookTitle,
    bool CanCheckout,
    string? BlockingReason
);

public sealed record ReturnVerificationDto(
    string BookNumber,
    string BookTitle,
    string StudentCardNumber,
    string StudentFullName,
    DateTime LoanedAtUtc,
    bool CanReturn,
    string? BlockingReason
);

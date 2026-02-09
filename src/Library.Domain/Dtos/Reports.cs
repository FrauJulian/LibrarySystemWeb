namespace Library.Domain.Dtos;

public sealed record OverdueItemDto(
    string StudentCardNumber,
    string StudentFullName,
    string BookNumber,
    string BookTitle,
    DateTime LoanedAtUtc,
    int DaysOverdue
);

public sealed record StudentLoanBookDto(
    string BookNumber,
    string BookTitle,
    DateTime LoanedAtUtc
);

public sealed record StudentWithLoansDto(
    string StudentCardNumber,
    string StudentFullName,
    int ActiveLoans,
    IReadOnlyList<StudentLoanBookDto> Books
);

public sealed record MonthlyBookStatDto(
    int Year,
    int Month,
    string BookNumber,
    string BookTitle,
    int LoanCount
);

public sealed record DashboardStatsDto(
    int TotalBooks,
    int LoanedBooks,
    int TotalStudents,
    int OverdueLoans
);

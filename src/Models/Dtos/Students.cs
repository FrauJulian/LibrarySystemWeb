namespace Library.Models.Dtos;

public sealed record StudentListItemDto(
    int StudentId,
    string CardNumber,
    string FirstName,
    string LastName,
    bool IsActive,
    int ActiveLoanCount
);

public sealed record StudentDetailsDto(
    int StudentId,
    string CardNumber,
    string FirstName,
    string LastName,
    bool IsActive
);

public sealed record StudentUpsertDto(
    string CardNumber,
    string FirstName,
    string LastName,
    bool IsActive
);

public sealed record StudentSearchQuery(string? NameContains);
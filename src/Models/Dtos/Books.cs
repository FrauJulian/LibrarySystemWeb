namespace Library.Models.Dtos;

public sealed record BookListItemDto(
    int BookId,
    string BookNumber,
    string Title,
    string AuthorOrEditor,
    string SubjectName,
    string? Isbn,
    bool IsLoaned
);

public sealed record BookDetailsDto(
    int BookId,
    string BookNumber,
    string Title,
    string AuthorOrEditor,
    string SubjectName,
    int SubjectId,
    string? Isbn,
    string? Publisher,
    string? PublisherCity,
    DateOnly? PublishedOn
);

public sealed record BookUpsertDto(
    string BookNumber,
    string Title,
    string AuthorOrEditor,
    int SubjectId,
    string? Isbn,
    string? Publisher,
    string? PublisherCity,
    DateOnly? PublishedOn
);

public sealed record BookSearchQuery(string? TitleContains, string? AuthorContains, int? SubjectId);
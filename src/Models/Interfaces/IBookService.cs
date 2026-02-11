using Library.Models.Common;
using Library.Models.Dtos;

namespace Library.Models.Interfaces;

public interface IBookService
{
    Task<IReadOnlyList<BookListItemDto>> SearchAsync(BookSearchQuery query,
        CancellationToken cancellationToken = default);

    Task<Results<BookDetailsDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Results<BookDetailsDto>>
        GetByBookNumberAsync(string bookNumber, CancellationToken cancellationToken = default);

    Task<Results<int>> CreateAsync(BookUpsertDto dto, CancellationToken cancellationToken = default);
    Task<Results> UpdateAsync(int id, BookUpsertDto dto, CancellationToken cancellationToken = default);
    Task<Results> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(int SubjectId, string Name)>> GetSubjectsAsync(CancellationToken cancellationToken = default);
}
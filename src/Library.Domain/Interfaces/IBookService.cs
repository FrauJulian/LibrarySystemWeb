using Library.Domain.Common;
using Library.Domain.Dtos;

namespace Library.Domain.Interfaces;

public interface IBookService
{
    Task<IReadOnlyList<BookListItemDto>> SearchAsync(BookSearchQuery query, CancellationToken cancellationToken = default);
    Task<Result<BookDetailsDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<BookDetailsDto>> GetByBookNumberAsync(string bookNumber, CancellationToken cancellationToken = default);
    Task<Result<int>> CreateAsync(BookUpsertDto dto, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(int id, BookUpsertDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(int SubjectId, string Name)>> GetSubjectsAsync(CancellationToken cancellationToken = default);
}

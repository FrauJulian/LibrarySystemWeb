using Library.Domain.Common;
using Library.Domain.Dtos;

namespace Library.Domain.Interfaces;

public interface IStudentService
{
    Task<IReadOnlyList<StudentListItemDto>> SearchAsync(StudentSearchQuery query, CancellationToken cancellationToken = default);
    Task<Result<StudentDetailsDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<StudentDetailsDto>> GetByCardNumberAsync(string cardNumber, CancellationToken cancellationToken = default);
    Task<Result<int>> CreateAsync(StudentUpsertDto dto, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(int id, StudentUpsertDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeactivateAsync(int id, CancellationToken cancellationToken = default);
}

using Library.Domain.Common;
using Library.Domain.Dtos;

namespace Library.Domain.Interfaces;

public interface IStudentService
{
    Task<IReadOnlyList<StudentListItemDto>> SearchAsync(StudentSearchQuery query, CancellationToken cancellationToken = default);
    Task<Results<StudentDetailsDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Results<StudentDetailsDto>> GetByCardNumberAsync(string cardNumber, CancellationToken cancellationToken = default);
    Task<Results<int>> CreateAsync(StudentUpsertDto dto, CancellationToken cancellationToken = default);
    Task<Results> UpdateAsync(int id, StudentUpsertDto dto, CancellationToken cancellationToken = default);
    Task<Results> DeactivateAsync(int id, CancellationToken cancellationToken = default);
}

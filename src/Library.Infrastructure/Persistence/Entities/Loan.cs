namespace Library.Infrastructure.Persistence.Entities;

public sealed partial class Loan
{
    public int LoanId { get; set; }
    public int BookId { get; set; }
    public int StudentId { get; set; }
    public DateTime LoanedAtUtc { get; set; }
    public Book Book { get; set; } = null!;
    public Student Student { get; set; } = null!;
}

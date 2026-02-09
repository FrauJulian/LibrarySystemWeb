namespace Library.Infrastructure.Persistence.Entities;

public partial class Loan
{
    public int LoanId { get; set; }
    public int BookId { get; set; }
    public int StudentId { get; set; }
    public DateTime LoanedAtUtc { get; set; }

    public virtual Book Book { get; set; } = null!;
    public virtual Student Student { get; set; } = null!;
}

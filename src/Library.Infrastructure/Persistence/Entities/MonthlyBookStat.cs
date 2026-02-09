namespace Library.Infrastructure.Persistence.Entities;

public partial class MonthlyBookStat
{
    public int MonthlyBookStatId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int BookId { get; set; }
    public int LoanCount { get; set; }

    public virtual Book Book { get; set; } = null!;
}

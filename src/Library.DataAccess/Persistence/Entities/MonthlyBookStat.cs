namespace Library.DataAccess.Persistence.Entities;

public sealed class MonthlyBookStat
{
    public int MonthlyBookStatId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int BookId { get; set; }
    public int LoanCount { get; set; }
    public Book Book { get; set; } = null!;
}
namespace Library.Infrastructure.Persistence.Entities;

public class MonthlySubjectStat
{
    public int MonthlySubjectStatId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; } // 1-12
    public int SubjectId { get; set; }
    public int LoanCount { get; set; }
    public virtual Subject Subject { get; set; } = null!;
}
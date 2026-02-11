namespace Library.Infrastructure.Persistence.Entities;

public sealed partial class Book
{
    public int BookId { get; set; }
    public string BookNumber { get; set; } = null!; // xxxxx-jjjj
    public int SubjectId { get; set; }
    public string? Isbn { get; set; }
    public string Title { get; set; } = null!;
    public string AuthorOrEditor { get; set; } = null!;
    public string? Publisher { get; set; }
    public string? PublisherCity { get; set; }
    public DateOnly? PublishedOn { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Subject Subject { get; set; } = null!;
    public Loan? Loan { get; set; }
    public ICollection<MonthlyBookStat> MonthlyBookStats { get; set; } = new List<MonthlyBookStat>();
}

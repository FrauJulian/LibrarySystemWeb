namespace Library.Infrastructure.Persistence.Entities;

public sealed partial class Subject
{
    public int SubjectId { get; set; }
    public string Name { get; set; } = null!;

    public ICollection<Book> Books { get; set; } = new List<Book>();
}

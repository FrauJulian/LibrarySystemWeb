namespace Library.Infrastructure.Persistence.Entities;

public partial class Subject
{
    public int SubjectId { get; set; }
    public string Name { get; set; } = null!;

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}

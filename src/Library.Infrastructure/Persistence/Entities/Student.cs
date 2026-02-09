namespace Library.Infrastructure.Persistence.Entities;

public partial class Student
{
    public int StudentId { get; set; }
    public string CardNumber { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public bool IsActive { get; set; } = true;

    public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();

    public string FullName => $"{FirstName} {LastName}";
}

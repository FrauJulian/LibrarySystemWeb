using Library.DataAccess.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Library.DataAccess.Persistence;

public class LibraryDbContext(DbContextOptions<LibraryDbContext> options) : DbContext(options)
{
    public virtual DbSet<Student> Students => Set<Student>();
    public virtual DbSet<Book> Books => Set<Book>();
    public virtual DbSet<Subject> Subjects => Set<Subject>();
    public virtual DbSet<Loan> Loans => Set<Loan>();
    public virtual DbSet<MonthlyBookStat> MonthlyBookStats => Set<MonthlyBookStat>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("Students");
            entity.HasKey(e => e.StudentId);
            entity.Property(e => e.CardNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasIndex(e => e.CardNumber).IsUnique();
            entity.HasIndex(e => e.LastName);
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.ToTable("Subjects");
            entity.HasKey(e => e.SubjectId);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.ToTable("Books");
            entity.HasKey(e => e.BookId);

            entity.Property(e => e.BookNumber).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.AuthorOrEditor).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Isbn).HasMaxLength(20);
            entity.Property(e => e.Publisher).HasMaxLength(200);
            entity.Property(e => e.PublisherCity).HasMaxLength(100);
            entity.Property(e => e.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasIndex(e => e.BookNumber).IsUnique();
            entity.HasIndex(e => e.Title);
            entity.HasIndex(e => e.AuthorOrEditor);
            entity.HasIndex(e => e.SubjectId);

            entity.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "CK_Books_BookNumber_Format",
                    "BookNumber LIKE '[0-9][0-9][0-9][0-9][0-9]-[0-9][0-9][0-9][0-9]'"
                );
            });

            entity.HasOne(d => d.Subject)
                .WithMany(p => p.Books)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Loan)
                .WithOne(p => p.Book)
                .HasForeignKey<Loan>(p => p.BookId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Loan>(entity =>
        {
            entity.ToTable("Loans");
            entity.HasKey(e => e.LoanId);

            entity.Property(e => e.LoanedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasIndex(e => e.BookId).IsUnique();

            entity.HasOne(d => d.Student)
                .WithMany(p => p.Loans)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MonthlyBookStat>(entity =>
        {
            entity.ToTable("MonthlyBookStats");
            entity.HasKey(e => e.MonthlyBookStatId);

            entity.Property(e => e.Year).IsRequired();
            entity.Property(e => e.Month).IsRequired();
            entity.Property(e => e.LoanCount).HasDefaultValue(0);

            entity.HasIndex(e => new { e.Year, e.Month, e.BookId }).IsUnique();

            entity.HasOne(d => d.Book)
                .WithMany(p => p.MonthlyBookStats)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
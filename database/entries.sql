USE BSEVITALibrary;

SET NOCOUNT ON;

BEGIN TRY
BEGIN TRAN;

DELETE FROM dbo.MonthlyBookStats;
DELETE FROM dbo.Loans;
DELETE FROM dbo.Books;
DELETE FROM dbo.Subjects;
DELETE FROM dbo.Students;

DBCC CHECKIDENT ('dbo.MonthlyBookStats', RESEED, 0);
    DBCC CHECKIDENT ('dbo.Loans', RESEED, 0);
    DBCC CHECKIDENT ('dbo.Books', RESEED, 0);
    DBCC CHECKIDENT ('dbo.Subjects', RESEED, 0);
    DBCC CHECKIDENT ('dbo.Students', RESEED, 0);

    DECLARE @SubjectSource TABLE (SubjectOrder INT IDENTITY(1,1) NOT NULL PRIMARY KEY, [Name] NVARCHAR(100) NOT NULL);
    DECLARE @SubjectMap TABLE (SubjectOrder INT NOT NULL PRIMARY KEY, SubjectId INT NOT NULL);

    DECLARE @StudentSource TABLE (
        StudentOrder INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CardNumber NVARCHAR(50) NOT NULL UNIQUE,
        FirstName NVARCHAR(80) NOT NULL,
        LastName NVARCHAR(80) NOT NULL,
        IsActive BIT NOT NULL
    );
    DECLARE @StudentMap TABLE (StudentOrder INT NOT NULL PRIMARY KEY, StudentId INT NOT NULL, CardNumber NVARCHAR(50) NOT NULL UNIQUE);

    DECLARE @BookSource TABLE (
        BookOrder INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        BookNumber NVARCHAR(20) NOT NULL UNIQUE,
        SubjectOrder INT NOT NULL,
        Isbn NVARCHAR(13) NOT NULL,
        Title NVARCHAR(200) NOT NULL,
        AuthorOrEditor NVARCHAR(200) NOT NULL,
        Publisher NVARCHAR(200) NOT NULL,
        PublisherCity NVARCHAR(100) NOT NULL,
        PublishedOn DATE NOT NULL
    );
    DECLARE @BookMap TABLE (BookOrder INT NOT NULL PRIMARY KEY, BookId INT NOT NULL, BookNumber NVARCHAR(20) NOT NULL UNIQUE);

INSERT INTO @SubjectSource([Name]) VALUES
                                       (N'Informatik'),
                                       (N'Mathematik'),
                                       (N'Physik'),
                                       (N'Chemie'),
                                       (N'Biologie'),
                                       (N'Geschichte'),
                                       (N'Deutsch'),
                                       (N'Englisch'),
                                       (N'Wirtschaft'),
                                       (N'Philosophie');

INSERT INTO dbo.Subjects([Name])
SELECT src.[Name]
FROM @SubjectSource src
ORDER BY src.SubjectOrder;

INSERT INTO @SubjectMap(SubjectOrder, SubjectId)
SELECT src.SubjectOrder, target.SubjectId
FROM @SubjectSource src
         INNER JOIN dbo.Subjects target ON target.[Name] = src.[Name];

INSERT INTO @StudentSource(CardNumber, FirstName, LastName, IsActive) VALUES
                                                                          (N'AT-2026-000001', N'Anna', N'Huber', 1),
                                                                          (N'AT-2026-000002', N'Lukas', N'Bauer', 1),
                                                                          (N'AT-2026-000003', N'Sophie', N'Wagner', 1),
                                                                          (N'AT-2026-000004', N'Paul', N'Mayer', 1),
                                                                          (N'AT-2026-000005', N'Marie', N'Gruber', 1),
                                                                          (N'AT-2026-000006', N'Felix', N'Steiner', 1),
                                                                          (N'AT-2026-000007', N'Lea', N'Hofer', 1),
                                                                          (N'AT-2026-000008', N'Jakob', N'Leitner', 1),
                                                                          (N'AT-2026-000009', N'Laura', N'Pichler', 1),
                                                                          (N'AT-2026-000010', N'David', N'Fuchs', 1);

INSERT INTO dbo.Students(CardNumber, FirstName, LastName, IsActive)
SELECT CardNumber, FirstName, LastName, IsActive
FROM @StudentSource
ORDER BY StudentOrder;

INSERT INTO @StudentMap(StudentOrder, StudentId, CardNumber)
SELECT src.StudentOrder, target.StudentId, src.CardNumber
FROM @StudentSource src
         INNER JOIN dbo.Students target ON target.CardNumber = src.CardNumber;

INSERT INTO @BookSource(BookNumber, SubjectOrder, Isbn, Title, AuthorOrEditor, Publisher, PublisherCity, PublishedOn) VALUES
                                                                                                                          (N'00001-1949', 6, N'9780451524935', N'1984', N'George Orwell', N'Signet Classic', N'New York', '1949-06-08'),
                                                                                                                          (N'00002-1932', 6, N'9780060850524', N'Brave New World', N'Aldous Huxley', N'Harper Perennial Modern Classics', N'New York', '1932-01-01'),
                                                                                                                          (N'00003-1953', 6, N'9781451673319', N'Fahrenheit 451', N'Ray Bradbury', N'Simon & Schuster', N'New York', '1953-10-19'),
                                                                                                                          (N'00004-1937', 6, N'9780547928227', N'The Hobbit', N'J. R. R. Tolkien', N'Mariner Books', N'Boston', '1937-09-21'),
                                                                                                                          (N'00005-1954', 6, N'9780544003415', N'The Lord of the Rings', N'J. R. R. Tolkien', N'Mariner Books', N'Boston', '1954-07-29'),
                                                                                                                          (N'00006-1960', 6, N'9780061120084', N'To Kill a Mockingbird', N'Harper Lee', N'Harper Perennial Modern Classics', N'New York', '1960-07-11'),
                                                                                                                          (N'00007-1925', 6, N'9780743273565', N'The Great Gatsby', N'F. Scott Fitzgerald', N'Scribner', N'New York', '1925-04-10'),
                                                                                                                          (N'00008-1813', 6, N'9780141439518', N'Pride and Prejudice', N'Jane Austen', N'Penguin Classics', N'London', '1813-01-28'),
                                                                                                                          (N'00009-1866', 6, N'9780143058144', N'Crime and Punishment', N'Fyodor Dostoevsky', N'Penguin Classics', N'London', '1866-01-01'),
                                                                                                                          (N'00010-1951', 6, N'9780316769488', N'The Catcher in the Rye', N'J. D. Salinger', N'Little, Brown and Company', N'Boston', '1951-07-16'),
                                                                                                                          (N'00011-2008', 1, N'9780132350884', N'Clean Code', N'Robert C. Martin', N'Prentice Hall', N'Boston', '2008-08-01'),
                                                                                                                          (N'00012-1999', 1, N'9780201616224', N'The Pragmatic Programmer', N'Andrew Hunt; David Thomas', N'Addison-Wesley', N'Boston', '1999-10-20'),
                                                                                                                          (N'00013-1994', 1, N'9780201633610', N'Design Patterns', N'Erich Gamma; Richard Helm; Ralph Johnson; John Vlissides', N'Addison-Wesley', N'Boston', '1994-10-31'),
                                                                                                                          (N'00014-2017', 1, N'9781449373320', N'Designing Data-Intensive Applications', N'Martin Kleppmann', N'O''Reilly Media', N'Sebastopol', '2017-03-16'),
                                                                                                                          (N'00015-2009', 2, N'9780262033848', N'Introduction to Algorithms', N'Thomas H. Cormen; Charles E. Leiserson; Ronald L. Rivest; Clifford Stein', N'MIT Press', N'Cambridge', '2009-07-31'),
                                                                                                                          (N'00016-1979', 2, N'9780465026562', N'Gödel, Escher, Bach', N'Douglas R. Hofstadter', N'Basic Books', N'New York', '1979-02-05'),
                                                                                                                          (N'00017-1988', 3, N'9780553380163', N'A Brief History of Time', N'Stephen Hawking', N'Bantam', N'New York', '1988-04-01'),
                                                                                                                          (N'00018-1859', 5, N'9781509827695', N'On the Origin of Species', N'Charles Darwin', N'Penguin Classics', N'London', '1859-11-24'),
                                                                                                                          (N'00019-2011', 9, N'9780307887894', N'Thinking, Fast and Slow', N'Daniel Kahneman', N'Farrar, Straus and Giroux', N'New York', '2011-10-25'),
                                                                                                                          (N'00020-2011', 6, N'9780062316097', N'Sapiens', N'Yuval Noah Harari', N'Harper', N'New York', '2011-01-01'),
                                                                                                                          (N'00021-2015', 6, N'9780062464316', N'Homo Deus', N'Yuval Noah Harari', N'Harper', N'New York', '2015-01-01'),
                                                                                                                          (N'00022-2014', 9, N'9780812981605', N'Capital in the Twenty-First Century', N'Thomas Piketty', N'Belknap Press', N'Cambridge', '2013-08-31'),
                                                                                                                          (N'00023-2000', 7, N'9780684853528', N'On Writing', N'Stephen King', N'Scribner', N'New York', '2000-10-03'),
                                                                                                                          (N'00024-1946', 10, N'9780807014271', N'Man''s Search for Meaning', N'Viktor E. Frankl', N'Beacon Press', N'Boston', '1946-01-01'),
                                                                                                                          (N'00025-1605', 6, N'9780060934347', N'Don Quixote', N'Miguel de Cervantes', N'Harper Perennial Modern Classics', N'New York', '1605-01-16'),
                                                                                                                          (N'00026-1862', 6, N'9780451419439', N'Les Misérables', N'Victor Hugo', N'Signet Classics', N'New York', '1862-01-01'),
                                                                                                                          (N'00027-1869', 6, N'9781400079988', N'War and Peace', N'Leo Tolstoy', N'Vintage', N'New York', '1869-01-01'),
                                                                                                                          (N'00028-1818', 6, N'9780486282114', N'Frankenstein', N'Mary Shelley', N'Dover Publications', N'New York', '1818-01-01'),
                                                                                                                          (N'00029-1897', 6, N'9780486411095', N'Dracula', N'Bram Stoker', N'Dover Publications', N'New York', '1897-05-26'),
                                                                                                                          (N'00030-1984', 6, N'9780441569595', N'Neuromancer', N'William Gibson', N'Ace', N'New York', '1984-07-01');

INSERT INTO dbo.Books(BookNumber, SubjectId, Isbn, Title, AuthorOrEditor, Publisher, PublisherCity, PublishedOn)
SELECT
    books.BookNumber,
    subjects.SubjectId,
    books.Isbn,
    books.Title,
    books.AuthorOrEditor,
    books.Publisher,
    books.PublisherCity,
    books.PublishedOn
FROM @BookSource books
         INNER JOIN @SubjectMap subjects ON subjects.SubjectOrder = books.SubjectOrder
ORDER BY books.BookOrder;

INSERT INTO @BookMap(BookOrder, BookId, BookNumber)
SELECT books.BookOrder, target.BookId, books.BookNumber
FROM @BookSource books
         INNER JOIN dbo.Books target ON target.BookNumber = books.BookNumber;

;WITH LoanBooks AS (
    SELECT TOP (30) BookId, ROW_NUMBER() OVER (ORDER BY BookId) AS LoanRowNumber
    FROM @BookMap
    ORDER BY BookId
),
      LoanStudents AS (
          SELECT StudentOrder, StudentId
          FROM @StudentMap
      )
 INSERT INTO dbo.Loans(BookId, StudentId, LoanedAtUtc)
SELECT
    loanBooks.BookId,
    loanStudents.StudentId,
    CASE
        WHEN loanBooks.LoanRowNumber <= 25 THEN DATEADD(DAY, -(95 + (loanBooks.LoanRowNumber % 35)), SYSUTCDATETIME())
        ELSE DATEADD(DAY, -(7 + (loanBooks.LoanRowNumber % 14)), SYSUTCDATETIME())
        END AS LoanedAtUtc
FROM LoanBooks loanBooks
         INNER JOIN LoanStudents loanStudents
                    ON loanStudents.StudentOrder = ((loanBooks.LoanRowNumber - 1) % 10) + 1;

COMMIT TRAN;
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0 ROLLBACK TRAN;
    THROW;
END CATCH

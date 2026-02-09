IF DB_ID(N'BSEVITALibrary') IS NULL
BEGIN
    CREATE DATABASE BSEVITALibrary;
END
GO

USE BSEVITALibrary;
GO

IF OBJECT_ID('dbo.MonthlyBookStats', 'U') IS NOT NULL DROP TABLE dbo.MonthlyBookStats;
IF OBJECT_ID('dbo.Loans', 'U') IS NOT NULL DROP TABLE dbo.Loans;
IF OBJECT_ID('dbo.Books', 'U') IS NOT NULL DROP TABLE dbo.Books;
IF OBJECT_ID('dbo.Subjects', 'U') IS NOT NULL DROP TABLE dbo.Subjects;
IF OBJECT_ID('dbo.Students', 'U') IS NOT NULL DROP TABLE dbo.Students;
GO

CREATE TABLE dbo.Students
(
    StudentId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Students PRIMARY KEY,
    CardNumber  NVARCHAR(20) NOT NULL,
    FirstName   NVARCHAR(100) NOT NULL,
    LastName    NVARCHAR(100) NOT NULL,
    IsActive    BIT NOT NULL CONSTRAINT DF_Students_IsActive DEFAULT(1)
);
GO

CREATE UNIQUE INDEX UX_Students_CardNumber ON dbo.Students(CardNumber);
CREATE INDEX IX_Students_LastName ON dbo.Students(LastName);
GO

CREATE TABLE dbo.Subjects
(
    SubjectId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Subjects PRIMARY KEY,
    Name      NVARCHAR(100) NOT NULL
);
GO

CREATE UNIQUE INDEX UX_Subjects_Name ON dbo.Subjects(Name);
GO

CREATE TABLE dbo.Books
(
    BookId         INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Books PRIMARY KEY,
    BookNumber     NVARCHAR(10) NOT NULL,
    SubjectId      INT NOT NULL,
    Isbn           NVARCHAR(20) NULL,
    Title          NVARCHAR(200) NOT NULL,
    AuthorOrEditor NVARCHAR(200) NOT NULL,
    Publisher      NVARCHAR(200) NULL,
    PublisherCity  NVARCHAR(100) NULL,
    PublishedOn    DATE NULL,
    CreatedAtUtc   DATETIME2(0) NOT NULL CONSTRAINT DF_Books_CreatedAtUtc DEFAULT(SYSUTCDATETIME())
);
GO

ALTER TABLE dbo.Books
ADD CONSTRAINT CK_Books_BookNumber_Format
CHECK (BookNumber LIKE '[0-9][0-9][0-9][0-9][0-9]-[0-9][0-9][0-9][0-9]');
GO

ALTER TABLE dbo.Books
ADD CONSTRAINT FK_Books_Subjects
FOREIGN KEY (SubjectId) REFERENCES dbo.Subjects(SubjectId)
ON DELETE NO ACTION;
GO

CREATE UNIQUE INDEX UX_Books_BookNumber ON dbo.Books(BookNumber);
CREATE INDEX IX_Books_Title ON dbo.Books(Title);
CREATE INDEX IX_Books_AuthorOrEditor ON dbo.Books(AuthorOrEditor);
CREATE INDEX IX_Books_SubjectId ON dbo.Books(SubjectId);
GO

CREATE TABLE dbo.Loans
(
    LoanId      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Loans PRIMARY KEY,
    BookId      INT NOT NULL,
    StudentId   INT NOT NULL,
    LoanedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_Loans_LoanedAtUtc DEFAULT(SYSUTCDATETIME())
);
GO

ALTER TABLE dbo.Loans
ADD CONSTRAINT FK_Loans_Books
FOREIGN KEY (BookId) REFERENCES dbo.Books(BookId)
ON DELETE CASCADE;

ALTER TABLE dbo.Loans
ADD CONSTRAINT FK_Loans_Students
FOREIGN KEY (StudentId) REFERENCES dbo.Students(StudentId)
ON DELETE NO ACTION;
GO

CREATE UNIQUE INDEX UX_Loans_BookId ON dbo.Loans(BookId);
CREATE INDEX IX_Loans_StudentId ON dbo.Loans(StudentId);
GO

CREATE TABLE dbo.MonthlyBookStats
(
    MonthlyBookStatId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MonthlyBookStats PRIMARY KEY,
    [Year]   INT NOT NULL,
    [Month]  INT NOT NULL,
    BookId INT NOT NULL,
    LoanCount INT NOT NULL CONSTRAINT DF_MonthlyBookStats_LoanCount DEFAULT(0)
);
GO

ALTER TABLE dbo.MonthlyBookStats
ADD CONSTRAINT FK_MonthlyBookStats_Books
FOREIGN KEY (BookId) REFERENCES dbo.Books(BookId)
ON DELETE CASCADE;
GO

CREATE UNIQUE INDEX UX_MonthlyBookStats_YearMonthBook
ON dbo.MonthlyBookStats([Year], [Month], BookId);
GO

INSERT INTO dbo.Subjects(Name)
VALUES
(N'Roman'),
(N'Sachbuch'),
(N'Kinder & Jugend'),
(N'Biografie'),
(N'Science'),
(N'Geschichte'),
(N'Sprachen');
GO

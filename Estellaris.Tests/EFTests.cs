using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Estellaris.Core.Extensions;
using Estellaris.EF;
using Estellaris.EF.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Estellaris.Tests {
  internal class Book {
    public int Id { get; set; }
    public string Name { get; set; }
    public int Pages { get; set; }
    public Writter Writter { get; set; }
    public int WritterId { get; set; }
  }

  internal class Writter {
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Book> Books { get; set; } = new Collection<Book>();
  }

  internal class TestingDbContext : BaseDbContext {
    public TestingDbContext(DbContextOptions<BaseDbContext> options) : base(options) { }

    protected override Assembly GetMappingAssembly() {
      return Assembly.Load(new AssemblyName("Estellaris.Tests"));
    }
  }

  internal class BookMapping : IMapping<Book> {
    public void Map(EntityTypeBuilder<Book> model) {
      model.ToTable("Books");
      model.HasKey(_ => _.Id);
      model.Property(_ => _.Name).HasMaxLength(30).IsRequired();
      model.Property(_ => _.Pages);
      model.HasOne(_ => _.Writter).WithMany(_ => _.Books).HasForeignKey(_ => _.WritterId);
      model.HasIndex(_ => _.Name);
    }
  }

  internal class WritterMapping : IMapping<Writter> {
    public void Map(EntityTypeBuilder<Writter> model) {
      model.ToTable("Writters");
      model.HasKey(_ => _.Id);
      model.Property(_ => _.Name).HasMaxLength(250).IsRequired();
      model.HasIndex(_ => _.Name);
    }
  }

  internal class BooksRepository : BaseRepository<Book> {
    public BooksRepository(BaseDbContext context) : base(context) { }
  }

  internal class WrittersRepository : BaseRepository<Writter> {
    public WrittersRepository(BaseDbContext context) : base(context) { }
  }

  internal class BookAuthor {
    public string BookName { get; set; }
    public string Author { get; set; }
  }

  [TestClass]
  public class EFTests {
    [TestMethod]
    public void TestSave() {
      ExecInDatabase((options) => {
        using(var context = new TestingDbContext(options)) {
          var repository = new WrittersRepository(context);
          repository.Save(new Writter { Name = "John Doe" });
          repository.SaveRange(new [] {
            new Writter { Name = "Janne Doe" },
            new Writter { Name = "Dick Doe" }
          });
        }

        using(var context = new TestingDbContext(options)) {
          var repository = new WrittersRepository(context);
          var first = repository.Query().First();
          Assert.AreEqual(repository.Count(), 3);
          Assert.AreEqual(first.Id, 1);
          Assert.AreEqual(first.Name, "John Doe");
          Assert.IsTrue(first.Books.IsEmpty());
        }
      });
    }

    [TestMethod]
    public void TestSaveRelational() {
      ExecInDatabase((options) => {
        using(var context = new TestingDbContext(options)) {
          var repository = new WrittersRepository(context);
          var writter = new Writter { Name = "John Doe" };
          writter.Books.Add(new Book { Name = "Universe Mysteries", Pages = 42 });
          repository.Save(writter);
        }

        using(var context = new TestingDbContext(options)) {
          var writterRepository = new WrittersRepository(context);
          var bookRepository = new BooksRepository(context);
          var writter = writterRepository.Query().First();
          var book = bookRepository.Query().First();
          Assert.AreEqual(writterRepository.Count(), 1);
          Assert.AreEqual(bookRepository.Count(), 1);
          Assert.AreEqual(writter.Id, 1);
          Assert.AreEqual(writter.Name, "John Doe");
          Assert.AreEqual(book.Id, 1);
          Assert.AreEqual(book.Name, "Universe Mysteries");
          Assert.AreEqual(book.Pages, 42);
          Assert.AreEqual(book.WritterId, writter.Id);
        }
      });
    }

    [TestMethod]
    public void TestUpdate() {
      ExecInDatabase((options) => {
        using(var context = new TestingDbContext(options)) {
          var repository = new WrittersRepository(context);
          repository.Save(new Writter { Name = "John Doe" });
        }

        using(var context = new TestingDbContext(options)) {
          var repository = new WrittersRepository(context);
          var writter = repository.Query().First();
          writter.Name = "Janne Doe";
          repository.Update(writter);
        }

        using(var context = new TestingDbContext(options)) {
          var repository = new WrittersRepository(context);
          var writter = repository.Query().First();
          Assert.AreEqual(repository.Count(), 1);
          Assert.AreEqual(writter.Id, 1);
          Assert.AreEqual(writter.Name, "Janne Doe");
        }
      });
    }

    [TestMethod]
    public void TestUpdateRelational() {
      ExecInDatabase((options) => {
        using(var context = new TestingDbContext(options)) {
          var repository = new WrittersRepository(context);
          var writter = new Writter { Name = "John Doe" };
          writter.Books.Add(new Book { Name = "Universe Mysteries", Pages = 42 });
          repository.Save(writter);
        }

        using(var context = new TestingDbContext(options)) {
          var repository = new WrittersRepository(context);
          var writter = repository.Query().Include(_ => _.Books).First();
          var book = writter.Books.First();
          writter.Name = "Janne Doe";
          book.Name = "Stars of the Universe";
          book.Pages = 1000;
          repository.Update(writter);
        }

        using(var context = new TestingDbContext(options)) {
          var repository = new WrittersRepository(context);
          var writter = repository.Query().Include(_ => _.Books).First();
          var book = writter.Books.First();
          Assert.AreEqual(repository.Count(), 1);
          Assert.AreEqual(writter.Id, 1);
          Assert.AreEqual(writter.Name, "Janne Doe");
          Assert.AreEqual(book.Id, 1);
          Assert.AreEqual(book.Name, "Stars of the Universe");
          Assert.AreEqual(book.Pages, 1000);
        }
      });
    }

    [TestMethod]
    public void TestDelete() {
      ExecInDatabase((options) => {
        using(var context = new TestingDbContext(options)) {
          var repository = new WrittersRepository(context);
          repository.Save(new Writter { Name = "John Doe" });
        }

        using(var context = new TestingDbContext(options)) {
          var repository = new WrittersRepository(context);
          var writter = repository.Query().First();
          repository.Delete(writter);
        }

        using(var context = new TestingDbContext(options)) {
          var repository = new WrittersRepository(context);
          Assert.AreEqual(repository.Count(), 0);
        }
      });
    }

    [TestMethod]
    public void TestDeleteRelational() {
      ExecInDatabase((options) => {
        using(var context = new TestingDbContext(options)) {
          var repository = new WrittersRepository(context);
          var writter = new Writter { Name = "John Doe" };
          writter.Books.Add(new Book { Name = "Universe Mysteries", Pages = 42 });
          repository.Save(writter);
        }

        using(var context = new TestingDbContext(options)) {
          var repository = new WrittersRepository(context);
          var writter = repository.Query().First();
          repository.Delete(writter);
        }

        using(var context = new TestingDbContext(options)) {
          var writterRepository = new WrittersRepository(context);
          var bookRepository = new BooksRepository(context);
          Assert.AreEqual(writterRepository.Count(), 0);
          Assert.AreEqual(bookRepository.Count(), 0);
        }
      });
    }

    [TestMethod]
    public void TestDeleteAll() {
      ExecInDatabase((options) => {
        using(var context = new TestingDbContext(options)) {
          var repository = new WrittersRepository(context);
          repository.SaveRange(new [] {
            new Writter { Name = "John Doe" },
            new Writter { Name = "Janne Doe" },
            new Writter { Name = "Dick Doe" }
          });
        }

        using(var context = new TestingDbContext(options)) {
          var repository = new WrittersRepository(context);
          repository.DeleteAll();
        }

        using(var context = new TestingDbContext(options)) {
          var repository = new WrittersRepository(context);
          Assert.AreEqual(repository.Count(), 0);
        }
      });
    }

    [TestMethod]
    public void TestFindAll() {
      ExecInDatabase((options) => {
        using(var context = new TestingDbContext(options)) {
          var repository = new WrittersRepository(context);
          repository.SaveRange(new [] {
            new Writter { Name = "John Doe" },
            new Writter { Name = "Janne Doe" },
            new Writter { Name = "Dick Doe" }
          });
        }

        using(var context = new TestingDbContext(options)) {
          var repository = new WrittersRepository(context);
          var writters = repository.FindAll();
          var writtersNames = new [] { "John Doe", "Janne Doe", "Dick Doe" };
          Assert.AreEqual(writters.Count(), 3);
          Assert.IsTrue(writtersNames.SequenceEqual(writters.Select(_ => _.Name)));
        }
      });
    }

    [TestMethod]
    public void TestFind() {
      ExecInDatabase((options) => {
        using(var context = new TestingDbContext(options)) {
          var repository = new WrittersRepository(context);
          repository.SaveRange(new [] {
            new Writter { Name = "John Doe" },
            new Writter { Name = "Janne Doe" },
            new Writter { Name = "Dick Doe" }
          });
        }

        using(var context = new TestingDbContext(options)) {
          var repository = new WrittersRepository(context);
          var writter = repository.Find(_ => _.Name.StartsWith("Dick")).FirstOrDefault();
          Assert.IsNotNull(writter);
          Assert.AreEqual(writter.Name, "Dick Doe");
        }
      });
    }

    void ExecInDatabase(Action<DbContextOptions<BaseDbContext>> action) {
      var connection = new SqliteConnection("DataSource=:memory:");
      connection.Open();
      try {
        var options = new DbContextOptionsBuilder<BaseDbContext>().UseSqlite(connection).Options;
        using(var context = new TestingDbContext(options)) {
          context.Database.EnsureCreated();
        }
        action.Invoke(options);
      } finally {
        connection.Close();
      }
    }
  }
}
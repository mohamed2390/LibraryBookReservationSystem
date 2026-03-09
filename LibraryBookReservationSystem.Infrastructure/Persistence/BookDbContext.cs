using LibraryBookReservationSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryBookReservationSystem.Infrastructure.Persistence;

public class LibraryDbContext : DbContext
{
 
   public DbSet<Book> Books => Set<Book>();
   public DbSet<Reservation> Reservations => Set<Reservation>();

   public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
      modelBuilder.Entity<Reservation>()
          .HasOne(r => r.Book)
          .WithMany()
          .HasForeignKey(r => r.BookId);

      modelBuilder.Entity<Book>().HasData(
            new Book
            {
               Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
               Title = "The Great Gatsby",
               Author = "F. Scott Fitzgerald",
               Genre = "Fiction",
               IsAvailable = true
            },
            new Book
            {
               Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
               Title = "1984",
               Author = "George Orwell",
               Genre = "Dystopian",
               IsAvailable = true
            },
            new Book
            {
               Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
               Title = "To Kill a Mockingbird",
               Author = "Harper Lee",
               Genre = "Classic",
               IsAvailable = true
            },
            new Book
            {
               Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
               Title = "Pride and Prejudice",
               Author = "Jane Austen",
               Genre = "Romance",
               IsAvailable = true
            },
            new Book
            {
               Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
               Title = "The Catcher in the Rye",
               Author = "J.D. Salinger",
               Genre = "Fiction",
               IsAvailable = true
            },
            new Book { Id = Guid.Parse("66666666-6666-6666-6666-666666666666"), Title = "Harry Potter and the Philosopher's Stone", Author = "J.K. Rowling", Genre = "Fantasy", IsAvailable = true },
            new Book { Id = Guid.Parse("77777777-7777-7777-7777-777777777777"), Title = "The Hobbit", Author = "J.R.R. Tolkien", Genre = "Fantasy", IsAvailable = true },
            new Book { Id = Guid.Parse("88888888-8888-8888-8888-888888888888"), Title = "Dune", Author = "Frank Herbert", Genre = "Science Fiction", IsAvailable = true },
            new Book { Id = Guid.Parse("99999999-9999-9999-9999-999999999999"), Title = "The Alchemist", Author = "Paulo Coelho", Genre = "Fiction", IsAvailable = true },
            new Book { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Title = "Atomic Habits", Author = "James Clear", Genre = "Self-Help", IsAvailable = true },
            new Book { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Title = "The Silent Patient", Author = "Alex Michaelides", Genre = "Thriller", IsAvailable = true },
            new Book { Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), Title = "Project Hail Mary", Author = "Andy Weir", Genre = "Science Fiction", IsAvailable = true },
            new Book { Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), Title = "The Midnight Library", Author = "Matt Haig", Genre = "Fiction", IsAvailable = true },
            new Book { Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), Title = "Circe", Author = "Madeline Miller", Genre = "Fantasy", IsAvailable = true },
            new Book { Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"), Title = "It Ends With Us", Author = "Colleen Hoover", Genre = "Romance", IsAvailable = true }
        );
   }
}
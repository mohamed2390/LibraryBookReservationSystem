using BookService.Domain.Entities;
using BookService.Infrastructure.Persistence;
using BookService.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace BookService.Tests.Infrastructure.Persistence.Repositories;

public class BookRepositoryTests : IDisposable
{
   private readonly BookDbContext _context;
   private readonly BookRepository _sut;

   public BookRepositoryTests()
   {
      var options = new DbContextOptionsBuilder<BookDbContext>()
          .UseInMemoryDatabase(Guid.NewGuid().ToString())
          .Options;

      _context = new BookDbContext(options);
      _context.Database.EnsureCreated();
      _sut = new BookRepository(_context);
   }


   [Fact]
   public async Task HasActiveReservationsAsync_ShouldReturnTrue_WhenReservationExists()
   {
      var bookId = Guid.NewGuid();
      _context.Reservations.Add(new Reservation { BookId = bookId, MemberId = Guid.NewGuid() });
      await _context.SaveChangesAsync();

      var hasReservation = await _sut.HasActiveReservationsAsync(bookId);
      hasReservation.Should().BeTrue();
   }

   [Fact]
   public async Task SearchAsync_ShouldReturnAllSeededData_WhenNoFiltersProvided()
   { 

         var books = await _sut.SearchAsync(string.Empty, string.Empty);
         books.Should().NotBeEmpty();
         books.Count().Should().BeGreaterThan(5);
   }

   public void Dispose() => _context.Dispose();
}
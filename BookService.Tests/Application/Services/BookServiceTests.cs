using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using BookService.Application.DTOs;
using BookService.Application.Services;
using BookService.Domain.Entities;
using BookService.Domain.Interfaces;
using Xunit;

namespace BookService.Tests.Application.Services;

public class BookServiceTests
{
   private readonly Mock<IBookRepository> _mockRepo;
   private readonly Mock<ILogger<global::BookService.Application.Services.BookService>> _mockLogger;
   private readonly global::BookService.Application.Services.BookService _sut;

   public BookServiceTests()
   {
      _mockRepo = new Mock<IBookRepository>();
      _mockLogger = new Mock<ILogger<global::BookService.Application.Services.BookService>>();
      _sut = new global::BookService.Application.Services.BookService(_mockRepo.Object, _mockLogger.Object);
   }

   [Fact]
   public async Task UpdateBookAsync_ShouldThrow_WhenTryingToMarkReservedBookAsUnavailable()
   {
      // Arrange
      var bookId = Guid.NewGuid();
      var dto = new RequestBookDto { Title = "Test", Author = "Test", Genre = "Fiction", IsAvailable = false };

      var existingBook = new Book { Id = bookId, Title = "Test Book", IsAvailable = true };
      _mockRepo.Setup(r => r.GetByIdAsync(bookId)).ReturnsAsync(existingBook);
      _mockRepo.Setup(r => r.HasActiveReservationsAsync(bookId)).ReturnsAsync(true); // ← Business rule trigger

      // Act & Assert
      await _sut.Invoking(s => s.UpdateBookAsync(bookId, dto))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"*Cannot Update book*");
   }

   [Fact]
   public async Task AddBookAsync_ShouldCallRepository_AndReturnBookDto()
   {
      var dto = new RequestBookDto { Title = "New Book", Author = "Author", Genre = "Fiction" };
      _mockRepo.Setup(r => r.AddAsync(It.IsAny<Book>())).Returns(Task.CompletedTask);

      var result = await _sut.AddBookAsync(dto);

      result.Should().NotBeNull();
      result.Title.Should().Be("New Book");
      _mockRepo.Verify(r => r.AddAsync(It.IsAny<Book>()), Times.Once);
   }

   [Fact]
   public async Task ReserveBookAsync_ShouldThrow_WhenBookIsNotAvailable()
   {
      var bookId = Guid.NewGuid();
      _mockRepo.Setup(r => r.GetByIdAsync(bookId)).ReturnsAsync(new Book { IsAvailable = false });

      await _sut.Invoking(s => s.ReserveBookAsync(bookId, Guid.NewGuid()))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Book is not available");
   }
}
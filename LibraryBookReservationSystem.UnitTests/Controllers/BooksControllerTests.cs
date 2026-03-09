using LibraryBookReservationSystem.Application.DTOs;
using LibraryBookReservationSystem.Application.Interfaces;
using LibraryBookReservationSystem.Api.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace LibraryBookReservationSystem.Tests.Controllers;

public class BooksControllerTests
{
   private readonly Mock<IBookService> _mockService;
   private readonly BooksController _controller;
   private readonly ILogger<BooksController> _mockLogger;

   public BooksControllerTests()
   {
      _mockService = new Mock<IBookService>();
      _mockLogger = new Mock<ILogger<BooksController>>().Object;     
      _controller = new BooksController(_mockService.Object, _mockLogger);
   }

   #region Helper to simulate authenticated user with role
   private void SetUserWithRole(string role)
   {
      var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, role)
        };

      var identity = new ClaimsIdentity(claims, "TestAuth");
      var principal = new ClaimsPrincipal(identity);

      _controller.ControllerContext = new ControllerContext
      {
         HttpContext = new DefaultHttpContext { User = principal }
      };
   }
   #endregion

   // ====================== ADMIN TESTS ======================
   [Fact]
   public async Task AddBook_ShouldReturnCreated_WhenUserIsAdmin()
   {
      SetUserWithRole("Admin");

      var dto = new RequestBookDto { Title = "Test Book", Author = "Test Author", Genre = "Fiction" };
      var resultDto = new BookDto { Id = Guid.NewGuid(), Title = "Test Book" };
      _mockService.Setup(s => s.AddBookAsync(dto)).ReturnsAsync(resultDto);

      var result = await _controller.AddBook(dto);

      var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
      createdResult.StatusCode.Should().Be(201);
   }

   [Fact]
   public async Task UpdateBook_ShouldReturnOk_WhenUserIsAdmin()
   {
      SetUserWithRole("Admin");

      var dto = new RequestBookDto { Title = "Updated", Author = "Updated", Genre = "Fiction" };
      var resultDto = new BookDto { Id = Guid.NewGuid() };
      _mockService.Setup(s => s.UpdateBookAsync(It.IsAny<Guid>(), dto)).ReturnsAsync(resultDto);

      var result = await _controller.UpdateBook(Guid.NewGuid(), dto);

      result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
   }

   [Fact]
   public async Task DeleteBook_ShouldReturnNoContent_WhenUserIsAdmin()
   {
      SetUserWithRole("Admin");

      _mockService.Setup(s => s.DeleteBookAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);

      var result = await _controller.DeleteBook(Guid.NewGuid());

      result.Should().BeOfType<NoContentResult>().Which.StatusCode.Should().Be(204);
      _mockService.Verify(s => s.DeleteBookAsync(It.IsAny<Guid>()), Times.Once);
   }

   // ====================== MEMBER TESTS ======================
   // Helper to set a specific user id + role
   private void SetUserWithIdAndRole(Guid id, string role)
   {
      var claims = new List<Claim>
      {
         new Claim(ClaimTypes.NameIdentifier, id.ToString()),
         new Claim(ClaimTypes.Role, role)
      };

      var identity = new ClaimsIdentity(claims, "TestAuth");
      var principal = new ClaimsPrincipal(identity);

      _controller.ControllerContext = new ControllerContext
      {
         HttpContext = new DefaultHttpContext { User = principal }
      };
   }

   [Fact]
   public async Task ReserveBook_ShouldReturnOk_WhenUserIsMember()
   {
      var memberId = Guid.NewGuid();
      SetUserWithIdAndRole(memberId, "Member");

      var bookId = Guid.NewGuid();
      _mockService.Setup(s => s.ReserveBookAsync(bookId, memberId)).Returns(Task.CompletedTask);

      var result = await _controller.ReserveBook(bookId);

      var ok = result.Should().BeOfType<OkObjectResult>().Subject;
      ok.Value.Should().Be("Book reserved successfully");
      _mockService.Verify(s => s.ReserveBookAsync(bookId, memberId), Times.Once);
   }

   [Fact]
   public async Task GetMyReservedBooks_ShouldReturnPagedResult_ForAuthenticatedMember()
   {
      var memberId = Guid.NewGuid();
      SetUserWithIdAndRole(memberId, "Member");

      var expected = new PagedResult<BookDto>
      {
         Items = new List<BookDto> { new BookDto { Id = Guid.NewGuid(), Title = "T" } },
         Page = 1,
         PageSize = 10,
         TotalCount = 1
      };

      _mockService.Setup(s => s.GetReservedBooksAsync(memberId, 1, 10)).ReturnsAsync(expected);

      var actionResult = await _controller.GetMyReservedBooks(1, 10);

      var ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
      ok.Value.Should().BeEquivalentTo(expected);
      _mockService.Verify(s => s.GetReservedBooksAsync(memberId, 1, 10), Times.Once);
   }

   [Fact]
   public async Task Search_ShouldReturnList_WhenAuthenticated()
   {
      SetUserWithRole("Member"); // any authenticated role allowed

      var sample = new List<BookDto>
      {
         new BookDto { Id = Guid.NewGuid(), Title = "A" },
         new BookDto { Id = Guid.NewGuid(), Title = "B" }
      };

      _mockService.Setup(s => s.SearchBooksAsync("Fiction", "Author")).ReturnsAsync(sample);

      var result = await _controller.Search("Fiction", "Author");

      var ok = result.Should().BeOfType<OkObjectResult>().Subject;
      ok.Value.Should().BeEquivalentTo(sample);
      _mockService.Verify(s => s.SearchBooksAsync("Fiction", "Author"), Times.Once);
   }
}
using FluentAssertions;
using LibraryMemberSystem.Application.DTOs;
using LibraryMemberSystem.Application.Interfaces;
using LibraryMemeberSystem.Api.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace LibraryMemberSystem.Tests.Controllers;

public class MembersControllerTests
{
   private readonly Mock<IMemberService> _mockService;
   private readonly MembersController _controller;

   public MembersControllerTests()
   {
      _mockService = new Mock<IMemberService>();
      _controller = new MembersController(_mockService.Object);
   }

   [Fact]
   public async Task Register_ShouldReturnOk_WhenRegistrationSucceeds()
   {
      var dto = new RegisterDto { Username = "testuser", Email = "test@test.com", Password = "123456" };
      var response = "User registered: " + dto.Username + "Successfully";
      _mockService.Setup(s => s.RegisterAsync(dto)).ReturnsAsync(response);

      var result = await _controller.Register(dto);

      var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
      okResult.Value.Should().BeEquivalentTo(response);
   }

   [Fact]
   public async Task Register_ShouldReturnConflict_WithProblemDetails_WhenInvalidOperationExceptionIsThrown()
   {
      // Arrange
      var dto = new RegisterDto
      {
         Username = "duplicateuser",
         Email = "test@test.com",
         Password = "password123"
      };

      // Simulate the service throwing the exception (same as real code)
      _mockService.Setup(s => s.RegisterAsync(dto))
                  .ThrowsAsync(new InvalidOperationException("Username already exists"));

      // Act
      var result = await _controller.Register(dto);

      // Assert - Check it's Conflict (409) with ProblemDetails
      var conflictResult = result.Should()
                                 .BeOfType<ConflictObjectResult>()
                                 .Subject;

      conflictResult.StatusCode.Should().Be(409);

      var problemDetails = conflictResult.Value.Should()
                                               .BeOfType<ProblemDetails>()
                                               .Subject;

      problemDetails.Title.Should().Be("Conflict occurred, Bad Request");
      problemDetails.Detail.Should().Be("Username already exists");
      problemDetails.Status.Should().Be(StatusCodes.Status409Conflict);
   }

   [Fact]
   public async Task Login_ShouldReturnUnauthorized_WhenLoginFails()
   {
      _mockService.Setup(s => s.LoginAsync(It.IsAny<LoginDto>())).ReturnsAsync((AuthResponseDto?)null);

      var result = await _controller.Login(new LoginDto());

      result.Should().BeOfType<UnauthorizedObjectResult>();
   }
}
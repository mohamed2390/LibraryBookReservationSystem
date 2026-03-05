using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MemberService.Application.DTOs;
using MemberService.Application.Interfaces;
using MemberService.Controllers;
using Xunit;

namespace MemberService.Tests.Controllers;

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
      var dto = new RegisterDto { Username = "test", Email = "test@test.com", Password = "123456" };
      var response = new AuthResponseDto { Token = "token", Role = "Member" };
      _mockService.Setup(s => s.RegisterAsync(dto)).ReturnsAsync(response);

      var result = await _controller.Register(dto);

      var okResult = result as OkObjectResult;
      okResult.Should().NotBeNull();
      okResult!.Value.Should().BeEquivalentTo(response);
   }

   [Fact]
   public async Task Register_ShouldReturnBadRequest_WhenExceptionThrown()
   {
      _mockService.Setup(s => s.RegisterAsync(It.IsAny<RegisterDto>()))
                  .ThrowsAsync(new InvalidOperationException("Username already exists"));

      var result = await _controller.Register(new RegisterDto());

      var badRequest = result as BadRequestObjectResult;
      badRequest.Should().NotBeNull();
      badRequest!.Value.Should().Be("Username already exists");
   }

   [Fact]
   public async Task Login_ShouldReturnUnauthorized_WhenLoginFails()
   {
      _mockService.Setup(s => s.LoginAsync(It.IsAny<LoginDto>())).ReturnsAsync((AuthResponseDto?)null);

      var result = await _controller.Login(new LoginDto());

      result.Should().BeOfType<UnauthorizedObjectResult>();
   }
}
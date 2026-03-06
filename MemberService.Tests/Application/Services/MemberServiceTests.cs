using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MemberService.Application.DTOs;
using MemberService.Application.Interfaces;
using MemberService.Application.Services;
using MemberService.Domain.Entities;
using MemberService.Domain.Interfaces;
using Xunit;

namespace MemberService.Tests.Application.Services;

public class MemberServiceTests
{
   private readonly Mock<IMemberRepository> _mockRepo;
   private readonly Mock<ITokenService> _mockTokenService;
   private readonly Mock<IPasswordHasher> _mockPasswordHasher;
   private readonly Mock<ILogger<MemberService.Application.Services.MemberService>> _mockLogger;
   private readonly MemberService.Application.Services.MemberService _sut; 

   public MemberServiceTests()
   {
      _mockRepo = new Mock<IMemberRepository>();
      _mockTokenService = new Mock<ITokenService>();
      _mockPasswordHasher = new Mock<IPasswordHasher>(); 
      _mockLogger = new Mock<ILogger<MemberService.Application.Services.MemberService>>();
      _sut = new MemberService.Application.Services.MemberService(_mockRepo.Object, _mockTokenService.Object, _mockPasswordHasher.Object, _mockLogger.Object);
   }

   [Fact]
   public async Task RegisterAsync_ShouldSucceed_WhenUsernameIsUnique()
   {
      // Arrange
      var dto = new RegisterDto { Username = "testuser", Email = "test@test.com", Role = "Member",Password = "password123" };
      
      
      _mockRepo.Setup(r => r.ExistsAsync(dto.Username)).ReturnsAsync(false);
      _mockRepo.Setup(r => r.AddAsync(It.IsAny<Member>())).Returns(Task.CompletedTask);
      _mockTokenService.Setup(t => t.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                      .Returns("fake-jwt-token");

      // Act
      var result = await _sut.RegisterAsync(dto);

      // Assert
      result.Should().NotBeNull();
      result.Token.Should().Be("fake-jwt-token");
      result.Role.Should().Be("Member");
      _mockRepo.Verify(r => r.AddAsync(It.Is<Member>(m => m.Username == "testuser")), Times.Once);
   }

   [Fact]
   public async Task RegisterAsync_ShouldThrow_WhenUsernameExists()
   {
      // Arrange
      var dto = new RegisterDto { Username = "existing", Email = "test@test.com", Password = "password123" };
      _mockRepo.Setup(r => r.ExistsAsync(dto.Username)).ReturnsAsync(true);

      // Act & Assert
      await _sut.Invoking(s => s.RegisterAsync(dto))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Username already exists");
   }
   [Fact]
   public async Task RegisterAsync_ShouldThrow_WhenEmailAlreadyExists()
   {
      // Arrange
      var dto = new RegisterDto { Username = "newuser", Email = "duplicate@test.com", Password = "password123" };

      _mockRepo.Setup(r => r.ExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
      _mockRepo.Setup(r => r.ExistsByEmailAsync(dto.Email)).ReturnsAsync(true);   // ← simulates duplicate email

      // Act & Assert
      await _sut.Invoking(s => s.RegisterAsync(dto))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Email already exists");
   }
   [Fact]
   public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
   {
      // Arrange
      var dto = new LoginDto { Username = "admin", Password = "admin123" };
      var member = new Member { Id = Guid.NewGuid(), Username = "admin", PasswordHash = MemberServiceTestsHelper.HashPassword("admin123"), Role = "Admin" };
      _mockPasswordHasher.Setup(h => h.Verify(dto.Password, member.PasswordHash)).Returns(true);
      _mockRepo.Setup(r => r.GetByUsernameAsync(dto.Username)).ReturnsAsync(member);
      _mockTokenService.Setup(t => t.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                      .Returns("valid-token");

      // Act
      var result = await _sut.LoginAsync(dto);

      // Assert
      result.Should().NotBeNull();
      result!.Token.Should().Be("valid-token");
   }

   [Fact]
   public async Task LoginAsync_ShouldReturnNull_WhenCredentialsAreInvalid()
   {
      // Arrange
      var dto = new LoginDto { Username = "wrong", Password = "wrong" };
      _mockRepo.Setup(r => r.GetByUsernameAsync(dto.Username)).ReturnsAsync((Member?)null);

      // Act
      var result = await _sut.LoginAsync(dto);

      // Assert
      result.Should().BeNull();
   }
}

// Helper for consistent password hashing in tests
internal static class MemberServiceTestsHelper
{
   public static string HashPassword(string password)
   {
      using var sha = System.Security.Cryptography.SHA256.Create();
      var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
      return Convert.ToBase64String(bytes);
   }
}
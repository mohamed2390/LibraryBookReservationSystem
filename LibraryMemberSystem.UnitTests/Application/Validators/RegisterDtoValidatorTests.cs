using FluentAssertions;
using FluentValidation.TestHelper;
using LibraryMemberSystem.Application.DTOs;
using LibraryMemberSystem.Application.Validators;
using Xunit;

namespace LibraryMemberSystem.Tests.Application.Validators;

public class RegisterDtoValidatorTests
{
   private readonly RegisterDtoValidator _validator = new();

   [Fact]
   public void Should_HaveError_WhenUsernameIsEmpty()
   {
      var dto = new RegisterDto { Username = "", Email = "test@test.com", Password = "123456" };
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Username);
   }

   [Fact]
   public void Should_HaveError_WhenEmailIsInvalid()
   {
      var dto = new RegisterDto { Username = "test", Email = "invalid-email", Password = "123456" };
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Email);
   }

   [Fact]
   public void Should_NotHaveError_WhenAllFieldsAreValid()
   {
      var dto = new RegisterDto { Username = "testuser", Email = "test@test.com", Role= "Member", Password = "password123" };
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveAnyValidationErrors();
   }
}
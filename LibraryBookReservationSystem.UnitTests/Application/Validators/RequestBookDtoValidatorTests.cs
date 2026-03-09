using FluentAssertions;
using FluentValidation.TestHelper;
using LibraryBookReservationSystem.Application.DTOs;
using LibraryBookReservationSystem.Application.Validators;
using Xunit;

namespace BookService.Tests.Application.Validators;

public class RequestBookDtoValidatorTests
{
   private readonly RequestBookDtoValidator _validator = new();

   [Fact]
   public void Should_HaveError_WhenTitleIsEmpty()
   {
      var dto = new RequestBookDto { Title = "", Author = "Test", Genre = "Fiction" };
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Title);
   }

   [Fact]
   public void Should_HaveError_WhenGenreIsInvalid()
   {
      var dto = new RequestBookDto { Title = "Valid", Author = "Test", Genre = "InvalidGenre" };
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Genre);
   }

   [Fact]
   public void Should_Pass_WhenAllFieldsAreValid()
   {
      var dto = new RequestBookDto { Title = "The Great Book", Author = "Author Name", Genre = "Fiction" };
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveAnyValidationErrors();
   }
}
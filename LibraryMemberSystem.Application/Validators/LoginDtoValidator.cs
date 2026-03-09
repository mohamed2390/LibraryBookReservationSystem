using LibraryMemberSystem.Application.DTOs;
using FluentValidation;

namespace LibraryMemberSystem.Application.Validators;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
   public LoginDtoValidator()
   {
      RuleFor(x => x.Username)
          .NotEmpty().WithMessage("Username is required");

      RuleFor(x => x.Password)
          .NotEmpty().WithMessage("Password is required");
   }
}
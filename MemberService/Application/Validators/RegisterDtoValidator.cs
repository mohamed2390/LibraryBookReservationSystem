using FluentValidation;
using MemberService.Application.DTOs;

namespace MemberService.Application.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
   public RegisterDtoValidator()
   {
      RuleFor(x => x.Username)
          .NotEmpty().WithMessage("Username is required")
          .MinimumLength(3).WithMessage("Username must be at least 3 characters")
          .MaximumLength(50).WithMessage("Username cannot exceed 50 characters");

      RuleFor(x => x.Role)
          .NotEmpty().WithMessage("Role is required")
          .MinimumLength(3).WithMessage("Username must be at least 3 characters")
          .MaximumLength(15).WithMessage("Username cannot exceed 50 characters");

      RuleFor(x => x.Email)
          .NotEmpty().WithMessage("Email is required")
          .EmailAddress().WithMessage("Invalid email format");

      RuleFor(x => x.Password)
          .NotEmpty().WithMessage("Password is required")
          .MinimumLength(6).WithMessage("Password must be at least 6 characters");
   }
}
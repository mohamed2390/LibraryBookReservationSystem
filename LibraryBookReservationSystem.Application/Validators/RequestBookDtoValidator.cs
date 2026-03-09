using FluentValidation;
using LibraryBookReservationSystem.Application.DTOs;

namespace LibraryBookReservationSystem.Application.Validators;

public class RequestBookDtoValidator : AbstractValidator<RequestBookDto>
{
   public RequestBookDtoValidator()
   {
      RuleFor(x => x.Title)
          .NotEmpty().WithMessage("Title is required")
          .MinimumLength(3).WithMessage("Title must be at least 3 characters")
          .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

      RuleFor(x => x.Author)
          .NotEmpty().WithMessage("Author is required")
          .MaximumLength(100);

      RuleFor(x => x.Genre)
          .NotEmpty().WithMessage("Genre is required")
          .Must(BeValidGenre).WithMessage("Genre must be one of: Fiction, Classic, Romance, Fantasy, Sci-Fi, Thriller, Self-Help");
   }

   private bool BeValidGenre(string genre)
   {
      var validGenres = new[] { "Fiction", "Classic", "Romance", "Fantasy", "Sci-Fi", "Thriller", "Self-Help", "Dystopian" };
      return validGenres.Contains(genre, StringComparer.OrdinalIgnoreCase);
   }
}
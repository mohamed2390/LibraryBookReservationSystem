using BookService.Application.DTOs;
using BookService.Application.Interfaces;
using BookService.Domain.Entities;
using BookService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace BookService.Application.Services;

public class BookService : IBookService
{
   private readonly IBookRepository _repository;
   private readonly ILogger<BookService> _logger;

   public BookService(IBookRepository repository, ILogger<BookService> logger)
   {
      _repository = repository;
      _logger = logger;
   }

   public async Task<PagedResult<BookDto>> GetReservedBooksAsync(Guid memberId, int page, int pageSize)
   {
      var books = await _repository.GetReservedBooksByMemberAsync(memberId);
      var total = books.Count();
      var items = books.Skip((page - 1) * pageSize).Take(pageSize)
                       .Select(MapToDto);

      return new PagedResult<BookDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
   }

   public async Task<IEnumerable<BookDto>> SearchBooksAsync(string? genre, string? author)
       => (await _repository.SearchAsync(genre, author)).Select(MapToDto);

   public async Task<BookDto?> GetByIdAsync(Guid id)
   {
      var book = await _repository.GetByIdAsync(id);
      return book != null ? MapToDto(book) : null;
   }

   public async Task<BookDto> AddBookAsync(RequestBookDto dto)
   {
      var book = new Book
      {
         Title = dto.Title,
         Author = dto.Author,
         Genre = dto.Genre,
         IsAvailable = true
      };

      await _repository.AddAsync(book);
      _logger.LogInformation("Book added: {Title}", dto.Title);

      return MapToDto(book);   
   }

   public async Task<BookDto> UpdateBookAsync(Guid id, RequestBookDto dto)
   {
      var book = await _repository.GetByIdAsync(id)
          ?? throw new KeyNotFoundException($"Book with ID {id} not found");

      // ========================================================
      // BUSINESS RULE: Only allow update if book is NOT reserved
      // ========================================================
      if (await _repository.HasActiveReservationsAsync(id))
      {
         throw new InvalidOperationException(
             $"Cannot Update book." +
             "It currently has active reservation(s). " +
             "Please wait until all reservations are completed.");
      }

      // Normal update
      book.Title = dto.Title;
      book.Author = dto.Author;
      book.Genre = dto.Genre;
      book.IsAvailable = dto.IsAvailable;

      await _repository.UpdateAsync(book);

      _logger.LogInformation("Book updated: {Title} (ID: {Id}) | Available: {Available}",
          dto.Title, id, dto.IsAvailable);

      return MapToDto(book);
   }

   public async Task DeleteBookAsync(Guid id)
   {
      if (await _repository.HasActiveReservationsAsync(id))
      {
         throw new InvalidOperationException(
             $"Cannot Delete book." +
             "It currently has active reservation(s). " +
             "Please wait until all reservations are completed.");
      }
      var book = await _repository.GetByIdAsync(id)
          ?? throw new KeyNotFoundException($"Book with ID {id} not found"); await _repository.DeleteAsync(id);
   }

   public async Task ReserveBookAsync(Guid bookId, Guid memberId)
   {
      _logger.LogInformation("ReserveBookAsync start. bookId={bookId}, memberId={memberId}", bookId, memberId);
      var book = await _repository.GetByIdAsync(bookId) ?? throw new KeyNotFoundException("Book not found");
      if (!book.IsAvailable) 
      {
         var reason = "Book is not available";
         _logger.LogWarning("ReserveBookAsync could not reserve book {bookId} because {reason}", bookId, reason);
         throw new InvalidOperationException(reason);
      }
        
      if (await _repository.HasActiveReservationAsync(bookId, memberId))
      {
         var reason = "You already reserved this book";
         _logger.LogWarning("ReserveBookAsync could not reserve book {bookId} because {reason}", bookId, reason);
         throw new InvalidOperationException(reason);
      }

      book.IsAvailable = false;
      await _repository.UpdateAsync(book);

      var reservation = new Reservation { BookId = bookId, MemberId = memberId };
      await _repository.AddReservationAsync(reservation);

      _logger.LogInformation("ReserveBookAsync succeeded. bookId={bookId}, memberId={memberId}", bookId, memberId);
   }

   private static BookDto MapToDto(Book book) => new()
   {
      Id = book.Id,
      Title = book.Title,
      Author = book.Author,
      Genre = book.Genre,
      IsAvailable = book.IsAvailable
   };
}
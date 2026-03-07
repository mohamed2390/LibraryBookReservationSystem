using BookService.Application.DTOs;

namespace BookService.Application.Interfaces;

public interface IBookService
{
   Task<PagedResult<BookDto>> GetReservedBooksAsync(Guid memberId, int page, int pageSize);
   Task<IEnumerable<BookDto>> SearchBooksAsync(string? genre, string? author);
   Task<BookDto> AddBookAsync(RequestBookDto dto); // Admin only

   Task<BookDto?> GetByIdAsync(Guid id);// Admin only
   Task<BookDto> UpdateBookAsync(Guid id, RequestBookDto dto);  // Admin only
   Task DeleteBookAsync(Guid id); // Admin only
   Task ReserveBookAsync(Guid bookId, Guid memberId); // Member only
}
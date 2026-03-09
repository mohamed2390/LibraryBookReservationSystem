using LibraryBookReservationSystem.Core.Entities;

namespace LibraryBookReservationSystem.Core.Interfaces;

public interface IBookRepository
{
   Task<IEnumerable<Book>> GetAllAsync(int page = 1, int pageSize = 10);
   Task<IEnumerable<Book>> SearchAsync(string? genre, string? author);
   Task<Book?> GetByIdAsync(Guid id);
   Task AddAsync(Book book);
   Task UpdateAsync(Book book);
   Task DeleteAsync(Guid id);

   // Reservation methods
   Task<bool> HasActiveReservationAsync(Guid bookId, Guid memberId);
   Task<bool> HasActiveReservationsAsync(Guid bookId);
   Task AddReservationAsync(Reservation reservation);
   Task<IEnumerable<Book>> GetReservedBooksByMemberAsync(Guid memberId);
}
using Microsoft.EntityFrameworkCore;
using BookService.Domain.Entities;
using BookService.Domain.Interfaces;
using BookService.Infrastructure.Persistence;

namespace BookService.Infrastructure.Persistence.Repositories;

public class BookRepository : IBookRepository
{
   private readonly BookDbContext _context;

   public BookRepository(BookDbContext context) => _context = context;

   public async Task<IEnumerable<Book>> GetAllAsync(int page, int pageSize)
       => await _context.Books.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

   public async Task<IEnumerable<Book>> SearchAsync(string? genre, string? author)
   {
      var query = _context.Books.AsQueryable();
      if (!string.IsNullOrEmpty(genre)) query = query.Where(b => b.Genre.Contains(genre));
      if (!string.IsNullOrEmpty(author)) query = query.Where(b => b.Author.Contains(author));
      return await query.ToListAsync();
   }

   public async Task<Book?> GetByIdAsync(Guid id) => await _context.Books.FindAsync(id);

   public async Task AddAsync(Book book)
   {
      await _context.Books.AddAsync(book);
      await _context.SaveChangesAsync();
   }

   public async Task UpdateAsync(Book book)
   {
      _context.Books.Update(book);
      await _context.SaveChangesAsync();
   }

   public async Task DeleteAsync(Guid id)
   {
      var book = await GetByIdAsync(id);
      if (book != null)
      {
         _context.Books.Remove(book);
         await _context.SaveChangesAsync();
      }
   }

   public async Task<bool> HasActiveReservationAsync(Guid bookId, Guid memberId)
       => await _context.Reservations.AnyAsync(r => r.BookId == bookId && r.MemberId == memberId);

   public async Task<bool> HasActiveReservationsAsync(Guid bookId)
   {
      return await _context.Reservations
          .AnyAsync(r => r.BookId == bookId);
   }

   public async Task AddReservationAsync(Reservation reservation)
   {
      await _context.Reservations.AddAsync(reservation);
      await _context.SaveChangesAsync();
   }

   public async Task<IEnumerable<Book>> GetReservedBooksByMemberAsync(Guid memberId)
   {
      return await _context.Reservations
          .Where(r => r.MemberId == memberId)
          .Include(r => r.Book)
          .Select(r => r.Book)
          .ToListAsync();
   }
}
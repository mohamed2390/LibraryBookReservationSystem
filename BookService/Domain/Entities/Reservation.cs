namespace BookService.Domain.Entities;

public class Reservation
{
   public Guid Id { get; set; } = Guid.NewGuid();
   public Guid BookId { get; set; }
   public Guid MemberId { get; set; }
   public DateTime ReservationDate { get; set; } = DateTime.UtcNow;

   public Book Book { get; set; } = null!;
}
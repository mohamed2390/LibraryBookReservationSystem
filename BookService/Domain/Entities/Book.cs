namespace BookService.Domain.Entities;

public class Book
{
   public Guid Id { get; set; } = Guid.NewGuid();
   public string Title { get; set; } = string.Empty;
   public string Author { get; set; } = string.Empty;
   public string Genre { get; set; } = string.Empty;
   public bool IsAvailable { get; set; } = true;
}
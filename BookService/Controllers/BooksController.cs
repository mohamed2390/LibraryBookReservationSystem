using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using BookService.Application.DTOs;
using BookService.Application.Interfaces;

namespace BookService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // All endpoints require authentication
public class BooksController : ControllerBase
{
   private readonly IBookService _bookService;
   private readonly ILogger<BooksController> _logger;

   public BooksController(IBookService bookService, ILogger<BooksController> logger)
   {
      _bookService = bookService;
      _logger = logger;
   }

   // Search endpoint
   [HttpGet("search")]
   public async Task<IActionResult> Search([FromQuery] string? genre, [FromQuery] string? author)
   {
      _logger.LogInformation("Search called. Genre={genre}, Author={author}", genre, author);
      var books = await _bookService.SearchBooksAsync(genre, author);
    
      return Ok(books);
   }

   // Admin only
   [Authorize(Roles = "Admin")]
   [HttpPost]
   public async Task<IActionResult> AddBook([FromBody] RequestBookDto dto)
   {
      var book = await _bookService.AddBookAsync(dto);
      return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
   }

   [Authorize(Roles = "Admin")]
   [HttpPut("{id}")]
   public async Task<IActionResult> UpdateBook(Guid id, [FromBody] RequestBookDto dto)
   {
      var book = await _bookService.UpdateBookAsync(id, dto);
      return Ok(book);
   }

   [Authorize(Roles = "Admin")]
   [HttpDelete("{id}")]
   public async Task<IActionResult> DeleteBook(Guid id)
   {
      await _bookService.DeleteBookAsync(id);
      return NoContent();
   }

   // GET api/books/{id} - Get single book by ID
   [HttpGet("{id}")]
   [Authorize(Roles = "Admin")]
   public async Task<IActionResult> GetById(Guid id)
   {
      var book = await _bookService.GetByIdAsync(id);
      return book != null ? Ok(book) : NotFound();
   }

   // Reserve endpoint (Member only)
   [Authorize(Roles = "Member")]
   [HttpPost("{bookId}/reserve")]
   public async Task<IActionResult> ReserveBook(Guid bookId)
   {
      var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
      if (string.IsNullOrEmpty(claim))
      {
         _logger.LogWarning("ReserveBook: missing NameIdentifier claim for book {bookId}", bookId);
         return BadRequest("Invalid user");
      }

      var memberId = Guid.Parse(claim);
      _logger.LogInformation("ReserveBook called. bookId={bookId}, memberId={memberId}", bookId, memberId);

      await _bookService.ReserveBookAsync(bookId, memberId);
      _logger.LogInformation("Book {bookId} reserved by member {memberId}", bookId, memberId);
      return Ok("Book reserved successfully");
   }

   // GET: api/reserved-books → only books reserved by the authenticated user 
   [Authorize(Roles = "Member")]
   [HttpGet("reserve")]
   public async Task<ActionResult<PagedResult<BookDto>>> GetMyReservedBooks([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
   {
      var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
      var memberId = Guid.Parse(claim!);
      _logger.LogInformation("GetMyReservedBooks called. memberId={memberId}, page={page}, pageSize={pageSize}",
         memberId, page, pageSize);

      var result = await _bookService.GetReservedBooksAsync(memberId, page, pageSize);

      return Ok(result);
   }
}



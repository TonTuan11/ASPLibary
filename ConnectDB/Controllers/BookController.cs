using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConnectDB.Data;
using ConnectDB.Models;

namespace ConnectDB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly AppDbContext _context; 

        public BooksController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/books
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .ToListAsync();

            return Ok(data);
        }

        // GET: api/books/1
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null) return NotFound();

            return Ok(book);
        }

        // POST
        [HttpPost]
        public async Task<IActionResult> Post(Book model)
        {
            _context.Books.Add(model);
            await _context.SaveChangesAsync();
            return Ok(model);
        }

        // PUT
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Book model)
        {
            if (id != model.BookId) return BadRequest();

            _context.Entry(model).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
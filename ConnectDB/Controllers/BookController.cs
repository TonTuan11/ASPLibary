using ConnectDB.Data;
using ConnectDB.dto;
using ConnectDB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .ToListAsync();

            return Ok(data);
        }

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

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Post([FromForm] BookCreateDto dto, IFormFile? image)
        {
            var authorExists = await _context.Authors.AnyAsync(a => a.AuthorId == dto.AuthorId);
            var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == dto.CategoryId);

            if (!authorExists || !categoryExists)
                return BadRequest("Author hoặc Category không tồn tại");

            var model = new Book
            {
                Title = dto.Title,
                AuthorId = dto.AuthorId,
                CategoryId = dto.CategoryId,
                Stock = dto.Stock
            };

            if (image != null)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(folder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await image.CopyToAsync(stream);

                model.ImageUrl = $"{Request.Scheme}://{Request.Host}/images/{fileName}";
            }

            _context.Books.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Put(int id, [FromForm] BookUpdateDto dto, IFormFile? image)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(dto.Title))
                book.Title = dto.Title;

            if (dto.Stock.HasValue)
                book.Stock = dto.Stock.Value;

            if (dto.AuthorId.HasValue)
                book.AuthorId = dto.AuthorId.Value;

            if (dto.CategoryId.HasValue)
                book.CategoryId = dto.CategoryId.Value;

            if (image != null)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(folder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await image.CopyToAsync(stream);

                book.ImageUrl = $"{Request.Scheme}://{Request.Host}/images/{fileName}";
            }

            await _context.SaveChangesAsync();

            return Ok(book);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
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
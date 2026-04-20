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
        private readonly IWebHostEnvironment _env;

        public BooksController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ================= GET ALL =================
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .ToListAsync();

            return Ok(data);
        }

        // ================= GET BY ID =================
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
        public async Task<IActionResult> Post(
      [FromForm] BookCreateDto dto,
      IFormFile? image)
        {
            try
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
                    var rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var folder = Path.Combine(rootPath, "images");

                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(folder, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await image.CopyToAsync(stream);

                    model.ImageUrl = "/images/" + fileName;
                }

                _context.Books.Add(model);
                await _context.SaveChangesAsync();

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Put(
    int id,
    [FromForm] BookUpdateDto dto,
    IFormFile? image)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            try
            {
                if (!string.IsNullOrWhiteSpace(dto.Title))
                    book.Title = dto.Title;

                if (dto.Stock.HasValue && dto.Stock >= 0)
                    book.Stock = dto.Stock.Value;

                if (dto.AuthorId.HasValue && dto.AuthorId > 0)
                {
                    var authorExists = await _context.Authors.AnyAsync(a => a.AuthorId == dto.AuthorId);
                    if (!authorExists) return BadRequest("Author không tồn tại");

                    book.AuthorId = dto.AuthorId.Value;
                }

                if (dto.CategoryId.HasValue && dto.CategoryId > 0)
                {
                    var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == dto.CategoryId);
                    if (!categoryExists) return BadRequest("Category không tồn tại");

                    book.CategoryId = dto.CategoryId.Value;
                }

                if (image != null)
                {
                    var rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var folder = Path.Combine(rootPath, "images");

                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(folder, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await image.CopyToAsync(stream);

                    book.ImageUrl = "/images/" + fileName;
                }

                await _context.SaveChangesAsync();

                return Ok(book);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }
        }

        // ================= DELETE =================
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
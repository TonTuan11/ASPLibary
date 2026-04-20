using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
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
        private readonly Cloudinary _cloudinary;

        public BooksController(AppDbContext context, Cloudinary cloudinary)
        {
            _context = context;
            _cloudinary = cloudinary;
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

        // ================= CREATE =================
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Post([FromForm] BookCreateDto dto, IFormFile? image)
        {
            var authorExists = await _context.Authors.AnyAsync(a => a.AuthorId == dto.AuthorId);
            var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == dto.CategoryId);

            if (!authorExists || !categoryExists)
                return BadRequest("Author hoặc Category không tồn tại");

            var book = new Book
            {
                Title = dto.Title,
                AuthorId = dto.AuthorId,
                CategoryId = dto.CategoryId,
                Stock = dto.Stock
            };

            // ================= CLOUDINARY UPLOAD =================
            if (image != null)
            {
                var uploadResult = await _cloudinary.UploadAsync(
                    new ImageUploadParams
                    {
                        File = new FileDescription(image.FileName, image.OpenReadStream()),
                        Folder = "books"
                    }
                );

                book.ImageUrl = uploadResult.SecureUrl.ToString();
            }

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return Ok(book);
        }

        // ================= UPDATE =================
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
                var uploadResult = await _cloudinary.UploadAsync(
                    new ImageUploadParams
                    {
                        File = new FileDescription(image.FileName, image.OpenReadStream()),
                        Folder = "books"
                    }
                );

                book.ImageUrl = uploadResult.SecureUrl.ToString();
            }

            await _context.SaveChangesAsync();
            return Ok(book);
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
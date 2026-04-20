using ConnectDB.Data;
using ConnectDB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
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
        public async Task<IActionResult> Post([FromForm] Book model, IFormFile? image)
        {
            try
            {
               
                model.Author = null;
                model.Category = null;

            
                var authorExists = await _context.Authors.AnyAsync(a => a.AuthorId == model.AuthorId);
                var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == model.CategoryId);

                if (!authorExists || !categoryExists)
                    return BadRequest("Author hoặc Category không tồn tại");

             
                if (image != null)
                {
                    var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(folder, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await image.CopyToAsync(stream);

                    model.ImageUrl = "/images/" + fileName;
                }

             
                _context.Entry(model).State = EntityState.Added;

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
        public async Task<IActionResult> Put(int id, [FromForm] Book model, IFormFile? image)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            model.Author = null;
            model.Category = null;

            try
            {
                if (!string.IsNullOrEmpty(model.Title))
                    book.Title = model.Title;

                if (!string.IsNullOrEmpty(model.Description))
                    book.Description = model.Description;

                if (model.Stock >= 0)
                    book.Stock = model.Stock;

                if (model.AuthorId != 0)
                    book.AuthorId = model.AuthorId;

                if (model.CategoryId != 0)
                    book.CategoryId = model.CategoryId;

                if (image != null)
                {
                    var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

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
                return BadRequest(ex.Message);
            }
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
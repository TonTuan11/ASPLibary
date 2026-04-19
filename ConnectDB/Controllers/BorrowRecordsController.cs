using ConnectDB.Data;
using ConnectDB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace ConnectDB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BorrowRecordsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BorrowRecordsController(AppDbContext context)
        {
            _context = context;
        }

        private void UpdateOverdueStatus(List<BorrowRecord> records)
        {
            var now = DateTime.UtcNow;

            foreach (var r in records)
            {
                if (r.Status == "Returned") continue;

                var dueDate = r.BorrowDate.AddDays(14);

                if (now > dueDate)
                    r.Status = "Overdue";
                else
                    r.Status = "Borrowing";

                _context.Entry(r).State = EntityState.Modified;
            }
        }



        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _context.BorrowRecords
                .Include(b => b.Member)
                .Include(b => b.Book)
                .ToListAsync();

            UpdateOverdueStatus(data);
            await _context.SaveChangesAsync();

            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.BorrowRecords
                .Include(b => b.Member)
                .Include(b => b.Book)
                .FirstOrDefaultAsync(b => b.BorrowId == id);

            return data == null ? NotFound() : Ok(data);
        }


        // trả sách
        [HttpPost("return/{id}")]
        [Authorize(Roles = "USER,ADMIN")]
        public async Task<IActionResult> ReturnBook(int id)
        {
            var borrow = await _context.BorrowRecords
                .Include(b => b.Book)
                .FirstOrDefaultAsync(b => b.BorrowId == id);

            if (borrow == null)
                return NotFound();

            if (borrow.Status == "Returned")
                return BadRequest("Đã trả rồi");

            borrow.Status = "Returned";
            borrow.ReturnDate = DateTime.UtcNow;

            if (borrow.Book != null)
            {
                borrow.Book.Stock += 1;
                _context.Entry(borrow.Book).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();

            return Ok(borrow);
        }



        [HttpGet("my-books")]
        [Authorize(Roles = "USER,ADMIN")]
        public async Task<IActionResult> GetMyBooks()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
                return Unauthorized();

            var userId = int.Parse(userIdClaim);

            var data = await _context.BorrowRecords
                .Include(b => b.Book)
                .Where(b => b.MemberId == userId)
                .ToListAsync();

            UpdateOverdueStatus(data);
            await _context.SaveChangesAsync();

            return Ok(data);
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
                {
                    return BadRequest("Author hoặc Category không tồn tại");
                }

                if (image != null)
                {
                    var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    model.ImageUrl = "/images/" + fileName;
                }

                _context.Books.Add(model);
                await _context.SaveChangesAsync();

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Put(int id, BorrowRecord model)
        {
            if (id != model.BorrowId) return BadRequest();
            _context.Entry(model).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(model);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.BorrowRecords.FindAsync(id);
            if (data == null) return NotFound();
            _context.BorrowRecords.Remove(data);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
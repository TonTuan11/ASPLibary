using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConnectDB.Data;
using ConnectDB.Models;

namespace ConnectDB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MembersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get() =>
            Ok(await _context.Members.ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.Members.FindAsync(id);
            return data == null ? NotFound() : Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Post(Member model)
        {
     
            if (string.IsNullOrWhiteSpace(model.Role))
            {
                model.Role = "user";
            }

            if (model.JoinDate == default)
            {
                model.JoinDate = DateTime.Now;
            }

            _context.Members.Add(model);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                model.MemberId,
                model.FullName,
                model.Email,
                JoinDate = model.JoinDate.ToString("yyyy-MM-dd"),
                model.Role
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Member model)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(model.FullName))
                member.FullName = model.FullName;

            if (!string.IsNullOrWhiteSpace(model.Email))
                member.Email = model.Email;

            if (model.JoinDate != default)
                member.JoinDate = model.JoinDate;

            await _context.SaveChangesAsync();
            return Ok(member);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.Members.FindAsync(id);
            if (data == null) return NotFound();
            _context.Members.Remove(data);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
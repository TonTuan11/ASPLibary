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
    public class MembersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MembersController(AppDbContext context)
        {
            _context = context;
        }


        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }

        private string GetUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _context.Members
                .Select(x => new
                {
                    x.MemberId,
                    x.FullName,
                    x.Email,
                    JoinDate = x.JoinDate.ToString("yyyy-MM-dd"),
                    x.Role
                })
                .ToListAsync();

            return Ok(data);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var currentUserId = GetUserId();
            var role = GetUserRole();

            if (role != "ADMIN" && currentUserId != id)
                return Forbid();

            var data = await _context.Members.FindAsync(id);
            if (data == null) return NotFound();

            return Ok(new
            {
                data.MemberId,
                data.FullName,
                data.Email,
                JoinDate = data.JoinDate.ToString("yyyy-MM-dd"),
                data.Role
            });
        }

   
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Post(Member model)
        {
            if (string.IsNullOrWhiteSpace(model.FullName) ||
                string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.Password))
            {
                return BadRequest("Thiếu dữ liệu");
            }

            var exists = await _context.Members
                .AnyAsync(x => x.Email == model.Email);

            if (exists)
                return BadRequest("Email đã tồn tại");

            model.Role = string.IsNullOrWhiteSpace(model.Role)
                ? "USER"
                : model.Role.ToUpper();

            model.JoinDate = DateTime.Now;
            model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);

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
            var currentUserId = GetUserId();
            var role = GetUserRole();

            if (role != "ADMIN" && currentUserId != id)
                return Forbid();

            var member = await _context.Members.FindAsync(id);
            if (member == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(model.FullName))
                member.FullName = model.FullName;

            if (!string.IsNullOrWhiteSpace(model.Email))
                member.Email = model.Email;

            if (!string.IsNullOrWhiteSpace(model.Password))
                member.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);

        
            if (role == "ADMIN" && !string.IsNullOrWhiteSpace(model.Role))
                member.Role = model.Role.ToUpper();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                member.MemberId,
                member.FullName,
                member.Email,
                member.Role
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.Members.FindAsync(id);
            if (data == null) return NotFound();

            _context.Members.Remove(data);
            await _context.SaveChangesAsync();

            return Ok("Xóa thành công");
        }
    }
}
using ConnectDB.Data;
using ConnectDB.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ConnectDB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // register
        [HttpPost("register")]
        public async Task<IActionResult> Register(Member request)
        {
            var exists = await _context.Members
                .AnyAsync(x => x.Email == request.Email);

            if (exists)
                return BadRequest("Email đã tồn tại");

            var user = new Member
            {
                FullName = request.FullName,
                Email = request.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "USER",
                JoinDate = DateTime.UtcNow
            };

            _context.Members.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Register thành công");
        }

        // login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _context.Members
                .FirstOrDefaultAsync(x => x.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return Unauthorized("Sai tài khoản hoặc mật khẩu");
            }

            var token = GenerateJwt(user);

            return Ok(new { token });
        }


        [HttpPost("admin-login")]
        public async Task<IActionResult> AdminLogin(LoginRequest request)
        {
            var user = await _context.Members
                .FirstOrDefaultAsync(x => x.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return Unauthorized("Sai tài khoản hoặc mật khẩu");
            }

            if (user.Role != "ADMIN")
            {
                return Unauthorized("Bạn không có quyền admin");
            }

            var token = GenerateJwt(user);

            return Ok(new { token });
        }



        //  tạo jwt
        private string GenerateJwt(Member user)
        {
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, user.MemberId.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role)
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"])
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,             
                expires: DateTime.UtcNow.AddDays(7),     
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
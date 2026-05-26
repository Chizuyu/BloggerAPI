using BCrypt.Net;
using BloggerAPI.Data;
using BloggerAPI.DTOs.Auth;
using BloggerAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace BloggerAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public AuthController(ApiDbContext context)
        {
            _context = context;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<ActionResult<UserResponseDto>> Register(RegisterDto registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
            {
                return BadRequest("Username sudah digunakan.");
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Username = registerDto.Username,
                PasswordHash = passwordHash,
                DateOfBirth = registerDto.DateOfBirth,
                JoinDate = DateTime.UtcNow
            };


            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var response = new UserResponseDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.Username,
                DateOfBirth = user.DateOfBirth,
                JoinDate = user.JoinDate
            };

            return Ok(response);
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<ActionResult<UserResponseDto>> Login(LoginDto loginDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == loginDto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Username atau password salah." });
            }

            var response = new UserResponseDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.Username,
                DateOfBirth = user.DateOfBirth,
                JoinDate = user.JoinDate,
                Photo = user.Photo
            };

            return Ok(response);
        }
    }
}

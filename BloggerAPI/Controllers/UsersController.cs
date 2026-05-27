using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using BloggerAPI.Data;
using BloggerAPI.DTOs.Auth;

namespace BloggerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApiDbContext _context;
        public UsersController(ApiDbContext context)
        {
            _context = context;
        }


        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers([FromQuery] string? name)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(u =>
                    u.Username.Contains(name) ||
                    u.FirstName.Contains(name) ||
                    u.LastName.Contains(name) ||
                    (u.FirstName + " " + u.LastName).Contains(name));
            }

            var users = await query.ToListAsync();

            return Ok(users.Select(u => new UserResponseDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Username = u.Username,
                DateOfBirth = u.DateOfBirth,
                JoinDate = u.JoinDate,
                Photo = Path.GetFileName(u.Photo), // Sesuai spek Legacy
                Password = ""
            }));
        }

        //GET: api/Users/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUser(Guid id)
        {
            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Username = u.Username,
                    Password = null,
                    DateOfBirth = u.DateOfBirth,
                    JoinDate = u.JoinDate,
                    Photo = Path.GetFileName(u.Photo)
                })
                .SingleOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { message = "User tidak ditemukan" });
            }

            return Ok(user);
        }
    }
}

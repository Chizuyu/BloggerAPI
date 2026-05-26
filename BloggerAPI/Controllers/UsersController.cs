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
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
        {
            return await _context.Users
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Username = u.Username,
                    DateOfBirth = u.DateOfBirth,
                    JoinDate = u.JoinDate,
                    Photo = u.Photo
                }).ToListAsync();
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
                    DateOfBirth = u.DateOfBirth,
                    JoinDate = u.JoinDate,
                    Photo = u.Photo
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

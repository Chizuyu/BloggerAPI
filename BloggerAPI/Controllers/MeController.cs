using System.Security.Claims;
using BloggerAPI.Data;
using BloggerAPI.DTOs.Auth;
using BloggerAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BloggerAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MeController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public MeController(ApiDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<UserResponseDto>> GetMe()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return NotFound();

            return Ok(new UserResponseDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.Username,
                DateOfBirth = user.DateOfBirth,
                JoinDate = user.JoinDate,
                Photo = user.Photo
            });
        }
    }
}

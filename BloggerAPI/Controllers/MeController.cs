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

        //GET: api/Me
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

        //PUT: api/Me
        [HttpPut]
        public async Task<IActionResult> UpdateProfile(UpdateUserDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            if (!string.IsNullOrEmpty(dto.FirstName)) user.FirstName = dto.FirstName;
            if (!string.IsNullOrEmpty(dto.LastName)) user.LastName = dto.LastName;

            if (!string.IsNullOrEmpty(dto.Username) && dto.Username != user.Username)
            {
                if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                    return BadRequest("Username sudah digunakan.");
                user.Username = dto.Username;
            }

            if (!string.IsNullOrEmpty(dto.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        //POST: api/Me/photo

        [HttpPost("photo")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadPhoto([FromForm] UserPhotoUploadDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/users");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Photo.FileName)}";
            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.Photo.CopyToAsync(stream);
            }

            user.Photo = $"/uploads/users/{fileName}";
            await _context.SaveChangesAsync();

            return Ok(new { photo_url = user.Photo });
        }

        //GET: api/Me/post
        [HttpGet("post")]
        public async Task<ActionResult<IEnumerable<PostResponseDto>>> GetMyPosts()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var posts = await _context.Posts
                .Include(p => p.Category)
                .Include(p => p.User)
                .Where(p => p.UserId == userId)
                .ToListAsync();

            return Ok(posts.Select(p => new PostResponseDto(
                p.Id, p.Title, p.Content, p.Thumbnail,
                p.Category?.Name ?? "", p.User?.Username ?? "", p.CreatedAt)));
        }

        //GET: api/Me/post/liked
        [HttpGet("post/liked")]
        public async Task<ActionResult<IEnumerable<PostResponseDto>>> GetLikedPosts()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Query post yang disukai user ini menggunakan tabel PostLike
            var posts = await _context.Posts
                .Include(p => p.Category)
                .Include(p => p.User)
                .Where(p => p.Likes.Any(l => l.UserId == userId))
                .ToListAsync();

            return Ok(posts
                .Select(p => new PostResponseDto
                    (
                        p.Id, 
                        p.Title, 
                        p.Content, 
                        p.Thumbnail, 
                        p.Category?.Name ?? "", 
                        p.User?.Username ?? "", 
                        p.CreatedAt
                    )
                )
            );
        }
    }
}

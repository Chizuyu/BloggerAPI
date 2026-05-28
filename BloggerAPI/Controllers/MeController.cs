using BloggerAPI.Data;
using BloggerAPI.DTOs;
using BloggerAPI.DTOs.Auth;
using BloggerAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

            var followersCount = await _context.Follows.CountAsync(f => f.FollowingId == userId);
            var followingCount = await _context.Follows.CountAsync(f => f.FollowerId == userId);

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            return Ok(new UserResponseDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.Username,
                Photo = GetFileNameOnly(user.Photo), 
                DateOfBirth = user.DateOfBirth,
                JoinDate = user.JoinDate,
                FollowersCount = followersCount,
                FollowingCount = followingCount
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

            return Ok(posts
                .Select(p => new PostResponseDto
                    (
                        p.Id,
                        p.CategoryId,
                        p.UserId,
                        p.Title,
                        p.Content,
                        GetFileNameOnly(p.Thumbnail),
                        GetFileNameOnly(p.ImageContent),
                        p.CreatedAt,
                        0,
                        new UserResponseDto
                        {
                            Id = p.User!.Id,
                            Username = p.User.Username,
                            FirstName = p.User.FirstName,
                            LastName = p.User.LastName,
                            Photo = GetFileNameOnly(p.User.Photo)
                        },
                        new CategoryResponseDto(
                            p.Category!.Id,
                            p.Category.Name
                        )
                    )
                )
            );
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
                        p.CategoryId,
                        p.UserId,
                        p.Title,
                        p.Content,
                        GetFileNameOnly(p.Thumbnail),
                        GetFileNameOnly(p.ImageContent),
                        p.CreatedAt,
                        0,
                        new UserResponseDto
                        {
                            Id = p.User!.Id,
                            Username = p.User.Username,
                            FirstName = GetFileNameOnly(p.User.FirstName),
                            LastName = p.User.LastName,
                            Photo = p.User.Photo
                        },
                        new CategoryResponseDto(
                            p.Category!.Id,
                            p.Category.Name
                        )
                    )
                )
            );
        }

        //GET: /api/me/is-liked-post/{postId}
        [HttpGet("is-liked-post/{postId}")]
        public async Task<IActionResult> CheckIsLiked(Guid postId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isLiked = await _context.Set<PostLike>()
                .AnyAsync(l => l.PostId == postId && l.UserId == userId);

            return Ok(new { isLiked = isLiked });
        }

        [NonAction]
        private string? GetFileNameOnly(string? path) =>
        string.IsNullOrEmpty(path) ? path : Path.GetFileName(path);
    }
}

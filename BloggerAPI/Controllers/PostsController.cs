using BloggerAPI.DTOs;
using BloggerAPI.Entities;
using BloggerAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BloggerAPI.Entities;      
using BloggerAPI.Repositories;  
using BloggerAPI.DTOs;

namespace BloggerAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly IPostRepository _postRepo;

        public PostsController(IPostRepository postRepo) => _postRepo = postRepo;


        //GET: 
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<PostResponseDto>>> GetPosts([FromQuery] string? title, [FromQuery] Guid? categoryId)
        {
            var posts = await _postRepo.GetAllAsync(title, categoryId);

            var response = posts.Select(p => new PostResponseDto(
                p.Id,
                p.Title,
                p.Content,
                p.Thumbnail,
                p.Category?.Name ?? "Uncategorized",
                p.User?.Username ?? "Unknown",
                p.CreatedAt
            ));

            return Ok(response);
        }

        //POST: 
        [HttpPost]
        public async Task<ActionResult<PostResponseDto>> Create(PostCreateDto dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var post = new Post
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Content = dto.Content,
                CategoryId = dto.CategoryId,
                UserId = Guid.Parse(userIdClaim),
                CreatedAt = DateTime.UtcNow
            };

            await _postRepo.CreateAsync(post);
            await _postRepo.SaveChangesAsync();

            // Re-fetch untuk mendapatkan navigation property (Category Name)
            var createdPost = await _postRepo.GetByIdAsync(post.Id);

            return CreatedAtAction(nameof(GetPosts), new { id = post.Id }, new PostResponseDto(
                createdPost!.Id, createdPost.Title, createdPost.Content, createdPost.Thumbnail,
                createdPost.Category?.Name ?? "", createdPost.User?.Username ?? "", createdPost.CreatedAt
            ));
        }

        //POST: 
        [HttpPost("{postId}/thumbnail")]
        public async Task<IActionResult> UploadThumbnail(Guid postId, IFormFile file)
        {
            var post = await _postRepo.GetByIdAsync(postId);
            if (post == null) return NotFound("Post tidak ditemukan");

            // Validasi file (Opsional tapi disarankan)
            if (file == null || file.Length == 0) return BadRequest("File tidak valid");

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/thumbnails");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            post.Thumbnail = $"/uploads/thumbnails/{fileName}";
            await _postRepo.SaveChangesAsync();

            return Ok(new { thumbnail_url = post.Thumbnail });
        }
    }
}

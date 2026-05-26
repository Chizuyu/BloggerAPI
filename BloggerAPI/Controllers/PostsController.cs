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

        // GET /api/posts/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<PostResponseDto>> GetById(Guid id)
        {
            var p = await _postRepo.GetByIdAsync(id);
            if (p == null) return NotFound();

            return Ok(new PostResponseDto(
                p.Id, p.Title, p.Content, p.Thumbnail,
                p.Category?.Name ?? "", p.User?.Username ?? "", p.CreatedAt
            ));
        }

        // PUT /api/posts/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, PostUpdateDto dto)
        {
            var post = await _postRepo.GetByIdAsync(id);
            if (post == null) return NotFound();

            // Pastikan hanya pemilik yang bisa update
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (post.UserId != userId) return Forbid();

            post.Title = dto.Title;
            post.Content = dto.Content;
            post.CategoryId = dto.CategoryId;
            post.UpdatedAt = DateTime.UtcNow;

            await _postRepo.UpdateAsync(post);
            await _postRepo.SaveChangesAsync();
            return NoContent();
        }

        // DELETE /api/posts/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var post = await _postRepo.GetByIdAsync(id);
            if (post == null) return NotFound();

            await _postRepo.DeleteAsync(post);
            await _postRepo.SaveChangesAsync();
            return NoContent();
        }
    }
}

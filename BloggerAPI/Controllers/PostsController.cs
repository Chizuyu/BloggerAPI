using BloggerAPI.DTOs;
using BloggerAPI.DTOs;
using BloggerAPI.DTOs.Auth;
using BloggerAPI.Entities;
using BloggerAPI.Entities;      
using BloggerAPI.Repositories;
using BloggerAPI.Repositories;  
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
                p.CategoryId,
                p.UserId,
                p.Title,
                p.Content,
                p.Thumbnail,
                p.ImageContent,
                p.CreatedAt, 
                0,           
                new UserResponseDto { 
                    Id = p.User!.Id, 
                    Username = p.User.Username, 
                    FirstName = p.User.FirstName, 
                    LastName = p.User.LastName, 
                    Photo = p.User.Photo 
                },
                new CategoryResponseDto(
                    p.Category!.Id, 
                    p.Category.Name
                ) 
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

            return CreatedAtAction(nameof(GetPosts), new { id = post.Id }, 
                new PostResponseDto(
                createdPost!.Id,
                createdPost.CategoryId,
                createdPost.UserId,
                createdPost.Title,
                createdPost.Content,
                createdPost.Thumbnail,
                createdPost.ImageContent,
                createdPost.CreatedAt, 
                0,           
                new UserResponseDto { 
                    Id = createdPost.User!.Id, 
                    Username = createdPost.User.Username, 
                    FirstName = createdPost.User.FirstName, 
                    LastName = createdPost.User.LastName, 
                    Photo = createdPost.User.Photo 
                },
                new CategoryResponseDto(
                    createdPost.Category!.Id, 
                    createdPost.Category.Name
                    ) 
            ));
        }

        // GET /api/posts/{postId}/total-count
        [HttpGet("{postId}/total-count")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTotalCount(Guid postId)
        {
            var count = await _postRepo.GetLikeCountAsync(postId);
            return Ok(new { total_likes = count });
        }

        // GET /api/posts/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<PostResponseDto>> GetById(Guid id)
        {
            var p = await _postRepo.GetByIdAsync(id);
            if (p == null) return NotFound();

            var likeCount = await _postRepo.GetLikeCountAsync(p.Id);

            // Mapping agar persis seperti yang diminta Android
            return Ok(new PostResponseDto(
                p.Id, p.CategoryId, p.UserId, p.Title, p.Content, p.Thumbnail, p.ImageContent,
                p.CreatedAt, // Mapping ke 'Date'
                likeCount,
                new UserResponseDto
                { // Mapping ke objek 'User'
                    Id = p.User!.Id,
                    Username = p.User.Username,
                    FirstName = p.User.FirstName,
                    LastName = p.User.LastName,
                    Photo = p.User.Photo
                },
                new CategoryResponseDto { Id = p.Category!.Id, Name = p.Category.Name }
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

            // PROTEKSI: Cek apakah yang menghapus adalah pemiliknya
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (post.UserId != currentUserId) return Forbid(); // Android akan terima error 403

            await _postRepo.DeleteAsync(post);
            await _postRepo.SaveChangesAsync();
            return NoContent();
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

        [HttpPost("{postId}/image")]
        [Consumes("multipart/form-data")] 
        public async Task<IActionResult> UploadImageContent(Guid postId, [FromForm] PostImageUploadDto dto)
        {
            var photo = dto.Photo;

            var post = await _postRepo.GetByIdAsync(postId);
            if (post == null) return NotFound("Post tidak ditemukan");

            if (photo == null || photo.Length == 0) return BadRequest("File 'photo' kosong");

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/posts");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            post.ImageContent = $"/uploads/posts/{fileName}";
            await _postRepo.SaveChangesAsync();

            return Ok(new { url = post.ImageContent });
        }

        // POST /api/posts/like
        [HttpPost("like")]
        public async Task<IActionResult> LikePost(PostLikeRequestDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isLiked = await _postRepo.ToggleLikeAsync(dto.PostId, userId);
            await _postRepo.SaveChangesAsync();

            return Ok(new { liked = isLiked });
        }
    }
}

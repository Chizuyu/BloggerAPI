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

            var response = new List<PostResponseDto>();
            foreach (var p in posts)
            {
                var count = await _postRepo.GetLikeCountAsync(p.Id); 
                response.Add(new PostResponseDto(
                    p.Id, p.CategoryId, 
                    p.UserId, 
                    p.Title, 
                    p.Content,
                    GetFileNameOnly(p.Thumbnail),    
                    GetFileNameOnly(p.ImageContent), 
                    p.CreatedAt,
                    count,
                    new UserResponseDto
                    {
                        Id = p.User!.Id,
                        Username = p.User.Username,
                        FirstName = p.User.FirstName,
                        LastName = p.User.LastName,
                        Photo = GetFileNameOnly(p.User.Photo), 
                        DateOfBirth = p.User.DateOfBirth,
                        JoinDate = p.User.JoinDate
                    },
                    new CategoryResponseDto(p.Category!.Id, p.Category.Name)
                ));
            }
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
            return Ok(new { count = count });
        }

        // GET /api/posts/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<PostResponseDto>> GetById(Guid id)
        {
            var p = await _postRepo.GetByIdAsync(id);
            if (p == null) return NotFound();

            var likeCount = await _postRepo.GetLikeCountAsync(p.Id);

            return Ok(new PostResponseDto(
                p.Id, 
                p.CategoryId, 
                p.UserId, 
                p.Title, 
                p.Content,
                GetFileNameOnly(p.Thumbnail),
                GetFileNameOnly(p.ImageContent),
                p.CreatedAt,
                likeCount,
                new UserResponseDto
                {
                    Id = p.User!.Id,
                    Username = p.User.Username,
                    FirstName = p.User.FirstName,
                    LastName = p.User.LastName,
                    Photo = GetFileNameOnly(p.User.Photo), 
                    DateOfBirth = p.User.DateOfBirth,
                    JoinDate = p.User.JoinDate
                },
                new CategoryResponseDto(p.Category!.Id, p.Category.Name)
            ));
        }

        // PUT /api/posts/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, PostUpdateDto dto)
        {
            var post = await _postRepo.GetByIdAsync(id);
            if (post == null) return NotFound();

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

            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (
                post.UserId != currentUserId
                ) return Forbid(); 

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

            return Ok(new { thumbnail_url = GetFileNameOnly(post.Thumbnail) });
        }

        //POST: 
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

            return Ok(new { url = GetFileNameOnly(post.ImageContent) });
        }

        // POST /api/posts/like
        [HttpPost("like")]
        public async Task<IActionResult> LikePost(PostLikeRequestDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var post = await _postRepo.GetByIdAsync(dto.PostId);
            if (post == null) return NotFound("Postingan tidak ditemukan");

            if (post.UserId == userId)
            {
                return BadRequest(new { message = "Anda tidak dapat menyukai postingan Anda sendiri" });
            }

            var isLiked = await _postRepo.ToggleLikeAsync(dto.PostId, userId);
            await _postRepo.SaveChangesAsync();

            return Ok(new { liked = isLiked });
        }

        [NonAction]
        private string? GetFileNameOnly(string? path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            return Path.GetFileName(path); 
        }
    }
}

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
    }
}

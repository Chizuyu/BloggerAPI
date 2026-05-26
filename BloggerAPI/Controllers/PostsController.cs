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

        public PostsController(IPostRepository postRepo)
        {
            _postRepo = postRepo;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<PostResponseDto>>> GetPosts([FromQuery] string? title, [FromQuery] Guid? categoryId)
        {
            var posts = await _postRepo.GetAllAsync(title, categoryId);

            // Manual mapping seperti di AuthController kamu
            var response = posts.Select(p => new PostResponseDto(
                p.Id, p.Title, p.Content, p.Thumbnail,
                p.Category?.Name ?? "", p.User?.Username ?? "", p.CreatedAt
            ));

            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<PostResponseDto>> Create(PostRequestDto dto)
        {
            // Cara mengambil ID User dari JWT Token (seperti di AuthController kamu)
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var post = new Post
            {
                Title = dto.Title,
                Content = dto.Content,
                CategoryId = dto.CategoryId,
                UserId = Guid.Parse(userIdClaim)
            };

            await _postRepo.CreateAsync(post);
            await _postRepo.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPosts), new { id = post.Id }, post);
        }
    }
}

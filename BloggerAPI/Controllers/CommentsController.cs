using BloggerAPI.DTOs;
using BloggerAPI.DTOs.Auth;
using BloggerAPI.Entities;
using BloggerAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BloggerAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/posts/{postId}/comments")]
    public class CommentsController : ControllerBase
    {
        private readonly IPostRepository _postRepo;
        public CommentsController(IPostRepository postRepo) => _postRepo = postRepo;

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CommentResponseDto>>> GetComments(Guid postId)
        {
            var comments = await _postRepo.GetCommentsByPostIdAsync(postId);
            return Ok(comments.Select(c => new CommentResponseDto(
                c.Id, 
                c.Content, 
                c.CreatedAt,
                new UserResponseDto { 
                    Id = c.User!.Id, 
                    Username = c.User.Username, 
                    FirstName = c.User.FirstName,
                    LastName = c.User.LastName,
                    Photo = Path.GetFileName(c.User.Photo) 
                }
            )));
        }

        [HttpPost]
        public async Task<IActionResult> PostComment(Guid postId, CommentCreateDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var comment = new Comment
            {
                Content = dto.Content,
                PostId = postId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _postRepo.CreateCommentAsync(comment);
            await _postRepo.SaveChangesAsync();
            return Ok();
        }
    }
}

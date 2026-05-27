using BloggerAPI.Data;
using BloggerAPI.DTOs;
using BloggerAPI.DTOs.Auth;
using BloggerAPI.Entities;
using BloggerAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BloggerAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/posts/{postId}/comments")]
    public class CommentsController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly IPostRepository _postRepo;
        public CommentsController(IPostRepository postRepo) => _postRepo = postRepo;

        //GET: api/posts/{postsId}/comments
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

        //POST: api/posts/{postsId}/comments
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

        //DELETE: api/posts/{postsId}/comments
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var comment = await _context.Comments
                .Include(c => c.Post) 
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null) return NotFound("Komentar tidak ditemukan");

            bool isCommentOwner = comment.UserId == userId;
            bool isPostOwner = comment.Post?.UserId == userId;

            if (!isCommentOwner && !isPostOwner)
            {
                return Forbid("Anda tidak memiliki hak untuk menghapus komentar ini.");
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

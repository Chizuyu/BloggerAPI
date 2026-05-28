using BloggerAPI.Data;
using BloggerAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BloggerAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class FollowsController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public FollowsController(ApiDbContext context)
        {
            _context = context;
        }

        [HttpPost("toggle/{targetUserId}")]
        public async Task<IActionResult> ToggleFollow(Guid targetUserId)
        {
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (currentUserId == targetUserId) return BadRequest("Cannot follow yourself");

            var existing = await _context.Follows.FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FollowingId == targetUserId);

            if (existing != null)
            {
                _context.Follows.Remove(existing);
                await _context.SaveChangesAsync();
                return Ok(new { isFollowing = false });
            }

            _context.Follows.Add(new Follow { FollowerId = currentUserId, FollowingId = targetUserId });
            await _context.SaveChangesAsync();
            return Ok(new { isFollowing = true });
        }


        [HttpGet("following/{userId}")]
        public async Task<IActionResult> GetFollowing(Guid userId)
        {
            var data = await _context.Follows
                .Where(f => f.FollowerId == userId)
                .Select(f => new {
                    userId = f.FollowingId,
                    username = f.Following.Username,
                    fullName = f.Following.FirstName + " " + f.Following.LastName,
                    photo = Path.GetFileName(f.Following.Photo) 
                }).ToListAsync();
            return Ok(data);
        }

        [HttpGet("followers/{userId}")]
        public async Task<IActionResult> GetFollowers(Guid userId)
        {
            var data = await _context.Follows
                .Where(f => f.FollowingId == userId)
                .Select(f => new {
                    userId = f.FollowerId,
                    username = f.Follower.Username,
                    fullName = f.Follower.FirstName + " " + f.Follower.LastName,
                    photo = Path.GetFileName(f.Follower.Photo)
                }).ToListAsync();
            return Ok(data);
        }
    }
}

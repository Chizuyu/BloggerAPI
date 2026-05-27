using BloggerAPI.Data;
using BloggerAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BloggerAPI.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly ApiDbContext _context;

        public PostRepository(ApiDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Post>> GetAllAsync(string? title, Guid? categoryId)
        {
            var query = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
            {
                query = query.Where(p => p.Title.Contains(title) || p.Content.Contains(title));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }

        public async Task<Post?> GetByIdAsync(Guid id)
        {
            return await _context.Posts
                .Include(p => p.Category)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<int> GetLikeCountAsync(Guid postId)
        => await _context.Set<PostLike>().CountAsync(l => l.PostId == postId);

        public async Task<bool> ToggleLikeAsync(Guid postId, Guid userId)
        {
            var existingLike = await _context.Set<PostLike>()
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (existingLike != null)
            {
                _context.Set<PostLike>().Remove(existingLike);
                return false; // Unlike
            }

            await _context.Set<PostLike>().AddAsync(new PostLike { PostId = postId, UserId = userId });
            return true; // Like
        }

        public async Task CreateAsync(Post post) => await _context.Posts.AddAsync(post);
        public async Task UpdateAsync(Post post) => _context.Posts.Update(post);
        public async Task DeleteAsync(Post post) => _context.Posts.Remove(post);
        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(Guid postId) =>
        await _context.Comments
            .Include(c => c.User)
            .Where(c => c.PostId == postId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        public async Task CreateCommentAsync(Comment comment) => await _context.Comments.AddAsync(comment);
        public async Task<Comment?> GetCommentByIdAsync(Guid id) => await _context.Comments.FindAsync(id);
        public async Task DeleteCommentAsync(Comment comment) => _context.Comments.Remove(comment);


    }
}

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
                .Include(p => p.Category)
                .Include(p => p.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(title))
                query = query.Where(p => p.Title.Contains(title));

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);

            return await query.ToListAsync();
        }

        public async Task<Post?> GetByIdAsync(Guid id)
        {
            return await _context.Posts
                .Include(p => p.Category)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task CreateAsync(Post post) => await _context.Posts.AddAsync(post);
        public async Task UpdateAsync(Post post) => _context.Posts.Update(post);
        public async Task DeleteAsync(Post post) => _context.Posts.Remove(post);
        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}

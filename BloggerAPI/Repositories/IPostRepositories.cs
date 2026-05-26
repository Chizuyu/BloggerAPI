using BloggerAPI.Entities;

namespace BloggerAPI.Repositories
{
    public interface IPostRepositories
    {
        Task<IEnumerable<Post>> GetAllAsync(string? title, Guid? categoryId);
        Task<Post?> GetByIdAsync(Guid id);
        Task CreateAsync(Post post);
        Task UpdateAsync(Post post);
        Task DeleteAsync(Post post);
        Task SaveChangesAsync();
    }
}

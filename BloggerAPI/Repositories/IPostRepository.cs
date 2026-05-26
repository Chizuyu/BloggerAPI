using BloggerAPI.Entities;

namespace BloggerAPI.Repositories
{
    public interface IPostRepository
    {
        Task<IEnumerable<Post>> GetAllAsync(string? title, Guid? categoryId);
        Task<Post?> GetByIdAsync(Guid id);
        Task CreateAsync(Post post);
        Task UpdateAsync(Post post);
        Task DeleteAsync(Post post);
        Task SaveChangesAsync();
        Task<int> GetLikeCountAsync(Guid postId);
        Task<bool> ToggleLikeAsync(Guid postId, Guid userId);
    }
}

using BloggerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BloggerAPI.Data
{
    public class ApiDbContext : DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
        {

        }
        public DbSet<Category> Categories { get; set; }

        public DbSet<User> Users { get; set; }
    }
}

using BloggerAPI.Entities;
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

        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Comment> Comments => Set<Comment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Posts)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Follow>()
                .HasKey(f => new { f.FollowerId, f.FollowingId });

            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Follower)
                .WithMany() 
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<Follow>()
                .HasOne(f => f.Following)
                .WithMany() 
                .HasForeignKey(f => f.FollowingId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

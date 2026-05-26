using System.ComponentModel.DataAnnotations;

namespace BloggerAPI.Entities
{
    public class Comment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid PostId { get; set; }
        public Post? Post { get; set; }
        public Guid UserId { get; set; }
        public User? User { get; set; }
    }
}

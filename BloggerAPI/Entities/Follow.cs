namespace BloggerAPI.Entities
{
    public class Follow
    {
        public Guid FollowerId { get; set; }
        public Guid Follower { get; set; }

        public Guid FollowingId { get; set; }
        public User Following { get; set; }

        public DateTime FollowedAt { get; set; } = DateTime.UtcNow;

    }
}

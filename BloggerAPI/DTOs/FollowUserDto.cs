namespace BloggerAPI.DTOs
{
    public class FollowUserDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Photo { get; set; }
    }
}

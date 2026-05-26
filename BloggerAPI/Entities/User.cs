namespace BloggerAPI.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string? FirstName{ get; set; }
        public string? LastName { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // Di DB namanya PasswordHash
        public DateTime? DateOfBirth { get; set; }
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;
        public string? Photo { get; set; }
    }
}

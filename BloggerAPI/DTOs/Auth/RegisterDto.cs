namespace BloggerAPI.DTOs.Auth
{
    public class RegisterDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
    }
}

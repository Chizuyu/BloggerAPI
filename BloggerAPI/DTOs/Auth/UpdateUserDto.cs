namespace BloggerAPI.DTOs.Auth
{
    public record UpdateUserDto(
    string? FirstName,
    string? LastName,
    string? Username,
    string? Password
    );

    public class UserPhotoUploadDto
    {
        public IFormFile Photo { get; set; } = null!;
    }
}

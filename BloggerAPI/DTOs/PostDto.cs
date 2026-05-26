using BloggerAPI.DTOs.Auth;
using System.ComponentModel.DataAnnotations;

namespace BloggerAPI.DTOs
{
    public record PostCreateDto(string Title, string Content, Guid CategoryId);
    public record PostUpdateDto(string Title, string Content, Guid CategoryId);

    public record PostRequestDto(
       [Required] string Title,
       [Required] string Content,
       [Required] Guid CategoryId
   );

    public record PostResponseDto(
        Guid Id,
        Guid CategoryId,
        Guid UserId,
        string? Title,
        string? Content,
        string? Thumbnail,
        string? ImageContent,
        DateTime Date, 
        int LikeCount,
        UserResponseDto User,
        CategoryResponseDto Category 
    );

    public record PostLikeRequestDto(
        Guid PostId
    );

    public class PostImageUploadDto
    {
        [Required]
        public IFormFile Photo { get; set; } = null!;
    }
}

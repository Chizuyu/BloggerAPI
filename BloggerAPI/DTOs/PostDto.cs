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
        string Title,
        string Content,
        string? Thumbnail,
        string CategoryName,
        string AuthorName,
        DateTime CreatedAt
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

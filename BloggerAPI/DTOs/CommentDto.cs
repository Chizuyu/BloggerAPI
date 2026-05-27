using BloggerAPI.DTOs.Auth;

namespace BloggerAPI.DTOs
{
    public record CommentCreateDto(string Content);
    public record CommentResponseDto(
        Guid Id,
        string Content,
        DateTime CreatedAt,
        UserResponseDto User 
    );
}

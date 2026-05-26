namespace BloggerAPI.DTOs
{
    public record PostCreateDto(string Title, string Content, Guid CategoryId);
    public record PostUpdateDto(string Title, string Content, Guid CategoryId);

    public record PostResponseDto(
        Guid Id,
        string Title,
        string Content,
        string? Thumbnail,
        string CategoryName,
        string AuthorName,
        DateTime CreatedAt
    );
}

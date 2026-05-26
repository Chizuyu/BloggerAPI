namespace BloggerAPI.DTOs
{
    public class CategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
    public record CategoryResponseDto(
        Guid Id,
        string? Name
    );
}

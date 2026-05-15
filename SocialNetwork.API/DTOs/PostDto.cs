namespace SocialNetwork.API.DTOs;

public class PostDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int AuthorId { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;
    public int? TimelineOwnerId { get; set; }
}

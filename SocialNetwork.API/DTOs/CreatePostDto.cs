namespace SocialNetwork.API.DTOs;

public class CreatePostDto
{
    public string Content { get; set; } = string.Empty;
    public int? TimelineOwnerId { get; set; }
}

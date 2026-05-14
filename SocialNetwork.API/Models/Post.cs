namespace SocialNetwork.API.Models;

public class Post
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;
    public int? TimelineOwnerId { get; set; }
    public User? TimelineOwner { get; set; }
}
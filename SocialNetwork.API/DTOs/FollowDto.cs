namespace SocialNetwork.API.DTOs;

public class FollowDto
{
    public int Id { get; set; }
    public int FollowerId { get; set; }
    public string FollowerUsername { get; set; } = string.Empty;
    public int FollowingId { get; set; }
    public string FollowingUsername { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

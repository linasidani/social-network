namespace SocialNetwork.API.DTOs;

public class CreateDirectMessageDto
{
    public string Content { get; set; } = string.Empty;
    public int ReceiverId { get; set; }
}

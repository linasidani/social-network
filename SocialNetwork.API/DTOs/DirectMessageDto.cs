namespace SocialNetwork.API.DTOs;

public class DirectMessageDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int SenderId { get; set; }
    public string SenderUsername { get; set; } = string.Empty;
    public int ReceiverId { get; set; }
    public string ReceiverUsername { get; set; } = string.Empty;
}

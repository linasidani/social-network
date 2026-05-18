using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialNetwork.API.Data;
using SocialNetwork.API.DTOs;
using SocialNetwork.API.Models;

namespace SocialNetwork.API.Controllers;

[ApiController]
[Route("api/messages")]
public class DirectMessagesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<DirectMessagesController> _logger;

    public DirectMessagesController(AppDbContext context, ILogger<DirectMessagesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Send a direct message
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<DirectMessageDto>> SendMessage(CreateDirectMessageDto dto, [FromQuery] int senderId)
    {
        if (string.IsNullOrWhiteSpace(dto.Content))
        {
            return BadRequest("Content is required");
        }

        if (senderId == dto.ReceiverId)
        {
            return BadRequest("Cannot send message to yourself");
        }

        // Verify both users exist
        var sender = await _context.Users.FindAsync(senderId);
        var receiver = await _context.Users.FindAsync(dto.ReceiverId);

        if (sender == null || receiver == null)
        {
            return NotFound("One or both users not found");
        }

        var message = new DirectMessage
        {
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow,
            SenderId = senderId,
            ReceiverId = dto.ReceiverId
        };

        _context.DirectMessages.Add(message);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Message sent from {senderId} to {dto.ReceiverId}");

        return CreatedAtAction(nameof(GetMessage), new { id = message.Id }, MapToDto(message, sender.Username, receiver.Username));
    }

    /// <summary>
    /// Get a direct message by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<DirectMessageDto>> GetMessage(int id)
    {
        var message = await _context.DirectMessages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (message == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(message, message.Sender.Username, message.Receiver.Username));
    }

    /// <summary>
    /// Get conversation between two users
    /// </summary>
    [HttpGet("conversation/{userId1}/{userId2}")]
    public async Task<ActionResult<List<DirectMessageDto>>> GetConversation(int userId1, int userId2)
    {
        var messages = await _context.DirectMessages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => (m.SenderId == userId1 && m.ReceiverId == userId2) || 
                        (m.SenderId == userId2 && m.ReceiverId == userId1))
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        return Ok(messages.Select(m => MapToDto(m, m.Sender.Username, m.Receiver.Username)).ToList());
    }

    /// <summary>
    /// Get inbox for a user
    /// </summary>
    [HttpGet("inbox/{userId}")]
    public async Task<ActionResult<List<DirectMessageDto>>> GetInbox(int userId)
    {
        var messages = await _context.DirectMessages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => m.ReceiverId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return Ok(messages.Select(m => MapToDto(m, m.Sender.Username, m.Receiver.Username)).ToList());
    }

    /// <summary>
    /// Get sent messages for a user
    /// </summary>
    [HttpGet("sent/{userId}")]
    public async Task<ActionResult<List<DirectMessageDto>>> GetSentMessages(int userId)
    {
        var messages = await _context.DirectMessages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => m.SenderId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return Ok(messages.Select(m => MapToDto(m, m.Sender.Username, m.Receiver.Username)).ToList());
    }

    /// <summary>
    /// Delete a direct message
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMessage(int id)
    {
        var message = await _context.DirectMessages.FindAsync(id);
        if (message == null)
        {
            return NotFound();
        }

        _context.DirectMessages.Remove(message);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Message {id} deleted");

        return NoContent();
    }

    private DirectMessageDto MapToDto(DirectMessage message, string senderUsername, string receiverUsername)
    {
        return new DirectMessageDto
        {
            Id = message.Id,
            Content = message.Content,
            CreatedAt = message.CreatedAt,
            SenderId = message.SenderId,
            SenderUsername = senderUsername,
            ReceiverId = message.ReceiverId,
            ReceiverUsername = receiverUsername
        };
    }
}

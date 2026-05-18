using Microsoft.EntityFrameworkCore;
using SocialNetwork.API.Data;
using SocialNetwork.API.DTOs;
using SocialNetwork.API.Models;

namespace SocialNetwork.API.Services;

public class DirectMessageService
{
    private readonly AppDbContext _context;

    public DirectMessageService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResult<DirectMessageDto>> SendAsync(CreateDirectMessageDto dto, int senderId)
    {
        if (string.IsNullOrWhiteSpace(dto.Content))
        {
            return ServiceResult<DirectMessageDto>.BadRequest("Content is required");
        }

        if (senderId == dto.ReceiverId)
        {
            return ServiceResult<DirectMessageDto>.BadRequest("Cannot send message to yourself");
        }

        var sender = await _context.Users.FindAsync(senderId);
        var receiver = await _context.Users.FindAsync(dto.ReceiverId);

        if (sender == null || receiver == null)
        {
            return ServiceResult<DirectMessageDto>.NotFound("One or both users not found");
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

        return ServiceResult<DirectMessageDto>.Success(MapToDto(message, sender.Username, receiver.Username));
    }

    public async Task<DirectMessageDto?> GetByIdAsync(int id)
    {
        var message = await _context.DirectMessages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .FirstOrDefaultAsync(m => m.Id == id);

        return message == null ? null : MapToDto(message, message.Sender.Username, message.Receiver.Username);
    }

    public async Task<List<DirectMessageDto>> GetConversationAsync(int userId1, int userId2)
    {
        var messages = await _context.DirectMessages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => (m.SenderId == userId1 && m.ReceiverId == userId2) ||
                        (m.SenderId == userId2 && m.ReceiverId == userId1))
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        return messages.Select(m => MapToDto(m, m.Sender.Username, m.Receiver.Username)).ToList();
    }

    public async Task<List<DirectMessageDto>> GetInboxAsync(int userId)
    {
        var messages = await _context.DirectMessages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => m.ReceiverId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return messages.Select(m => MapToDto(m, m.Sender.Username, m.Receiver.Username)).ToList();
    }

    public async Task<List<DirectMessageDto>> GetSentAsync(int userId)
    {
        var messages = await _context.DirectMessages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => m.SenderId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return messages.Select(m => MapToDto(m, m.Sender.Username, m.Receiver.Username)).ToList();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var message = await _context.DirectMessages.FindAsync(id);
        if (message == null)
        {
            return false;
        }

        _context.DirectMessages.Remove(message);
        await _context.SaveChangesAsync();

        return true;
    }

    private static DirectMessageDto MapToDto(DirectMessage message, string senderUsername, string receiverUsername)
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

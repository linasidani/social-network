using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.API.DTOs;
using SocialNetwork.API.Services;

namespace SocialNetwork.API.Controllers;

[Authorize]
[ApiController]
[Route("api/messages")]
public class DirectMessagesController : ControllerBase
{
    private readonly ILogger<DirectMessagesController> _logger;
    private readonly DirectMessageService _messageService;

    public DirectMessagesController(DirectMessageService messageService, ILogger<DirectMessagesController> logger)
    {
        _logger = logger;
        _messageService = messageService;
    }

    /// <summary>
    /// Send a direct message
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<DirectMessageDto>> SendMessage(CreateDirectMessageDto dto, [FromQuery] int senderId)
    {
        var result = await _messageService.SendAsync(dto, senderId);
        if (!result.IsSuccess)
        {
            return ToActionResult(result);
        }

        _logger.LogInformation("Message sent from {SenderId} to {ReceiverId}", senderId, dto.ReceiverId);

        return CreatedAtAction(nameof(GetMessage), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Get a direct message by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<DirectMessageDto>> GetMessage(int id)
    {
        var message = await _messageService.GetByIdAsync(id);

        if (message == null)
        {
            return NotFound();
        }

        return Ok(message);
    }

    /// <summary>
    /// Get conversation between two users
    /// </summary>
    [HttpGet("conversation/{userId1}/{userId2}")]
    public async Task<ActionResult<List<DirectMessageDto>>> GetConversation(int userId1, int userId2)
    {
        return Ok(await _messageService.GetConversationAsync(userId1, userId2));
    }

    /// <summary>
    /// Get inbox for a user
    /// </summary>
    [HttpGet("inbox/{userId}")]
    public async Task<ActionResult<List<DirectMessageDto>>> GetInbox(int userId)
    {
        return Ok(await _messageService.GetInboxAsync(userId));
    }

    /// <summary>
    /// Get sent messages for a user
    /// </summary>
    [HttpGet("sent/{userId}")]
    public async Task<ActionResult<List<DirectMessageDto>>> GetSentMessages(int userId)
    {
        return Ok(await _messageService.GetSentAsync(userId));
    }

    /// <summary>
    /// Delete a direct message
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMessage(int id)
    {
        if (!await _messageService.DeleteAsync(id))
        {
            return NotFound();
        }

        _logger.LogInformation("Message {MessageId} deleted", id);

        return NoContent();
    }

    private ActionResult ToActionResult<T>(ServiceResult<T> result)
    {
        return result.Status switch
        {
            ServiceResultStatus.BadRequest => BadRequest(result.Error),
            ServiceResultStatus.NotFound => NotFound(result.Error),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}

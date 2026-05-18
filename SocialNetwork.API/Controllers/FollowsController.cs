using Microsoft.AspNetCore.Mvc;
using SocialNetwork.API.DTOs;
using SocialNetwork.API.Services;

namespace SocialNetwork.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FollowsController : ControllerBase
{
    private readonly ILogger<FollowsController> _logger;
    private readonly FollowService _followService;

    public FollowsController(FollowService followService, ILogger<FollowsController> logger)
    {
        _logger = logger;
        _followService = followService;
    }

    /// <summary>
    /// Follow a user
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<FollowDto>> FollowUser([FromQuery] int followerId, [FromQuery] int followingId)
    {
        var result = await _followService.FollowAsync(followerId, followingId);
        if (!result.IsSuccess)
        {
            return ToActionResult(result);
        }

        _logger.LogInformation("User {FollowerId} started following {FollowingId}", followerId, followingId);

        return CreatedAtAction(nameof(GetFollowersCount), new { userId = followingId }, result.Value);
    }

    /// <summary>
    /// Unfollow a user
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> UnfollowUser([FromQuery] int followerId, [FromQuery] int followingId)
    {
        if (!await _followService.UnfollowAsync(followerId, followingId))
        {
            return NotFound();
        }

        _logger.LogInformation("User {FollowerId} stopped following {FollowingId}", followerId, followingId);

        return NoContent();
    }

    /// <summary>
    /// Get followers of a user
    /// </summary>
    [HttpGet("followers/{userId}")]
    public async Task<ActionResult<List<UserDto>>> GetFollowers(int userId)
    {
        return Ok(await _followService.GetFollowersAsync(userId));
    }

    /// <summary>
    /// Get following list of a user
    /// </summary>
    [HttpGet("following/{userId}")]
    public async Task<ActionResult<List<UserDto>>> GetFollowing(int userId)
    {
        return Ok(await _followService.GetFollowingAsync(userId));
    }

    /// <summary>
    /// Get followers count
    /// </summary>
    [HttpGet("{userId}/followers-count")]
    public async Task<ActionResult<int>> GetFollowersCount(int userId)
    {
        return Ok(await _followService.GetFollowersCountAsync(userId));
    }

    /// <summary>
    /// Get following count
    /// </summary>
    [HttpGet("{userId}/following-count")]
    public async Task<ActionResult<int>> GetFollowingCount(int userId)
    {
        return Ok(await _followService.GetFollowingCountAsync(userId));
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

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialNetwork.API.Data;
using SocialNetwork.API.DTOs;
using SocialNetwork.API.Models;

namespace SocialNetwork.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FollowsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<FollowsController> _logger;

    public FollowsController(AppDbContext context, ILogger<FollowsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Follow a user
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<FollowDto>> FollowUser([FromQuery] int followerId, [FromQuery] int followingId)
    {
        if (followerId == followingId)
        {
            return BadRequest("Cannot follow yourself");
        }

        // Verify both users exist
        var follower = await _context.Users.FindAsync(followerId);
        var following = await _context.Users.FindAsync(followingId);

        if (follower == null || following == null)
        {
            return NotFound("One or both users not found");
        }

        // Check if already following
        if (await _context.Follows.AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId))
        {
            return BadRequest("Already following this user");
        }

        var follow = new Follow
        {
            FollowerId = followerId,
            FollowingId = followingId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Follows.Add(follow);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"User {followerId} started following {followingId}");

        return CreatedAtAction(nameof(GetFollowersCount), new { userId = followingId }, MapToDto(follow, follower.Username, following.Username));
    }

    /// <summary>
    /// Unfollow a user
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> UnfollowUser([FromQuery] int followerId, [FromQuery] int followingId)
    {
        var follow = await _context.Follows.FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
        if (follow == null)
        {
            return NotFound();
        }

        _context.Follows.Remove(follow);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"User {followerId} stopped following {followingId}");

        return NoContent();
    }

    /// <summary>
    /// Get followers of a user
    /// </summary>
    [HttpGet("followers/{userId}")]
    public async Task<ActionResult<List<UserDto>>> GetFollowers(int userId)
    {
        var followers = await _context.Follows
            .Include(f => f.Follower)
            .Where(f => f.FollowingId == userId)
            .Select(f => f.Follower)
            .ToListAsync();

        return Ok(followers.Select(MapUserToDto).ToList());
    }

    /// <summary>
    /// Get following list of a user
    /// </summary>
    [HttpGet("following/{userId}")]
    public async Task<ActionResult<List<UserDto>>> GetFollowing(int userId)
    {
        var following = await _context.Follows
            .Include(f => f.Following)
            .Where(f => f.FollowerId == userId)
            .Select(f => f.Following)
            .ToListAsync();

        return Ok(following.Select(MapUserToDto).ToList());
    }

    /// <summary>
    /// Get followers count
    /// </summary>
    [HttpGet("{userId}/followers-count")]
    public async Task<ActionResult<int>> GetFollowersCount(int userId)
    {
        var count = await _context.Follows.CountAsync(f => f.FollowingId == userId);
        return Ok(count);
    }

    /// <summary>
    /// Get following count
    /// </summary>
    [HttpGet("{userId}/following-count")]
    public async Task<ActionResult<int>> GetFollowingCount(int userId)
    {
        var count = await _context.Follows.CountAsync(f => f.FollowerId == userId);
        return Ok(count);
    }

    private FollowDto MapToDto(Follow follow, string followerUsername, string followingUsername)
    {
        return new FollowDto
        {
            Id = follow.Id,
            FollowerId = follow.FollowerId,
            FollowerUsername = followerUsername,
            FollowingId = follow.FollowingId,
            FollowingUsername = followingUsername,
            CreatedAt = follow.CreatedAt
        };
    }

    private UserDto MapUserToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        };
    }
}

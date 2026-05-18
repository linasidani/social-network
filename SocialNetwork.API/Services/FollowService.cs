using Microsoft.EntityFrameworkCore;
using SocialNetwork.API.Data;
using SocialNetwork.API.DTOs;
using SocialNetwork.API.Models;

namespace SocialNetwork.API.Services;

public class FollowService
{
    private readonly AppDbContext _context;

    public FollowService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResult<FollowDto>> FollowAsync(int followerId, int followingId)
    {
        if (followerId == followingId)
        {
            return ServiceResult<FollowDto>.BadRequest("Cannot follow yourself");
        }

        var follower = await _context.Users.FindAsync(followerId);
        var following = await _context.Users.FindAsync(followingId);

        if (follower == null || following == null)
        {
            return ServiceResult<FollowDto>.NotFound("One or both users not found");
        }

        if (await _context.Follows.AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId))
        {
            return ServiceResult<FollowDto>.BadRequest("Already following this user");
        }

        var follow = new Follow
        {
            FollowerId = followerId,
            FollowingId = followingId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Follows.Add(follow);
        await _context.SaveChangesAsync();

        return ServiceResult<FollowDto>.Success(MapToDto(follow, follower.Username, following.Username));
    }

    public async Task<bool> UnfollowAsync(int followerId, int followingId)
    {
        var follow = await _context.Follows.FirstOrDefaultAsync(f =>
            f.FollowerId == followerId && f.FollowingId == followingId);

        if (follow == null)
        {
            return false;
        }

        _context.Follows.Remove(follow);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<UserDto>> GetFollowersAsync(int userId)
    {
        var followers = await _context.Follows
            .Include(f => f.Follower)
            .Where(f => f.FollowingId == userId)
            .Select(f => f.Follower)
            .ToListAsync();

        return followers.Select(UserService.MapToDto).ToList();
    }

    public async Task<List<UserDto>> GetFollowingAsync(int userId)
    {
        var following = await _context.Follows
            .Include(f => f.Following)
            .Where(f => f.FollowerId == userId)
            .Select(f => f.Following)
            .ToListAsync();

        return following.Select(UserService.MapToDto).ToList();
    }

    public Task<int> GetFollowersCountAsync(int userId)
    {
        return _context.Follows.CountAsync(f => f.FollowingId == userId);
    }

    public Task<int> GetFollowingCountAsync(int userId)
    {
        return _context.Follows.CountAsync(f => f.FollowerId == userId);
    }

    private static FollowDto MapToDto(Follow follow, string followerUsername, string followingUsername)
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
}

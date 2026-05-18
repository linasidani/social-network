using Microsoft.EntityFrameworkCore;
using SocialNetwork.API.Data;
using SocialNetwork.API.Models;
using SocialNetwork.API.Services;

namespace SocialNetwork.Tests.Services;

public class FollowServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly FollowService _service;

    public FollowServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _service = new FollowService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task FollowAsync_WithValidUsers_ReturnsSuccess()
    {
        // Arrange
        var follower = new User { Username = "follower", Email = "f@example.com", PasswordHash = "hash" };
        var following = new User { Username = "following", Email = "g@example.com", PasswordHash = "hash" };
        _context.Users.AddRange(follower, following);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.FollowAsync(follower.Id, following.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(follower.Id, result.Value!.FollowerId);
        Assert.Equal(following.Id, result.Value.FollowingId);
    }

    [Fact]
    public async Task FollowAsync_WhenUserFollowsSelf_ReturnsBadRequest()
    {
        // Arrange
        var user = new User { Username = "user", Email = "u@example.com", PasswordHash = "hash" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.FollowAsync(user.Id, user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultStatus.BadRequest, result.Status);
    }

    [Fact]
    public async Task FollowAsync_WithInvalidFollower_ReturnsNotFound()
    {
        // Arrange
        var following = new User { Username = "following", Email = "g@example.com", PasswordHash = "hash" };
        _context.Users.Add(following);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.FollowAsync(999, following.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task FollowAsync_WithInvalidFollowing_ReturnsNotFound()
    {
        // Arrange
        var follower = new User { Username = "follower", Email = "f@example.com", PasswordHash = "hash" };
        _context.Users.Add(follower);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.FollowAsync(follower.Id, 999);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task FollowAsync_WhenAlreadyFollowing_ReturnsBadRequest()
    {
        // Arrange
        var follower = new User { Username = "follower", Email = "f@example.com", PasswordHash = "hash" };
        var following = new User { Username = "following", Email = "g@example.com", PasswordHash = "hash" };
        _context.Users.AddRange(follower, following);
        await _context.SaveChangesAsync();

        _context.Follows.Add(new Follow { FollowerId = follower.Id, FollowingId = following.Id, CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.FollowAsync(follower.Id, following.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultStatus.BadRequest, result.Status);
    }

    [Fact]
    public async Task FollowAsync_CircularRelation_NotBlockedByDefault()
    {
        // Arrange - A follows B, B follows A (circular, but allowed by current implementation)
        var userA = new User { Username = "userA", Email = "a@example.com", PasswordHash = "a" };
        var userB = new User { Username = "userB", Email = "b@example.com", PasswordHash = "b" };
        _context.Users.AddRange(userA, userB);
        await _context.SaveChangesAsync();

        // A follows B
        await _service.FollowAsync(userA.Id, userB.Id);

        // Act - B follows A
        var result = await _service.FollowAsync(userB.Id, userA.Id);

        // Assert - Current implementation allows this
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UnfollowAsync_WithExistingFollow_ReturnsTrue()
    {
        // Arrange
        var follower = new User { Username = "follower", Email = "f@example.com", PasswordHash = "hash" };
        var following = new User { Username = "following", Email = "g@example.com", PasswordHash = "hash" };
        _context.Users.AddRange(follower, following);
        await _context.SaveChangesAsync();

        _context.Follows.Add(new Follow { FollowerId = follower.Id, FollowingId = following.Id, CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.UnfollowAsync(follower.Id, following.Id);

        // Assert
        Assert.True(result);
        Assert.Empty(_context.Follows);
    }

    [Fact]
    public async Task UnfollowAsync_WithNonExistingFollow_ReturnsFalse()
    {
        // Act
        var result = await _service.UnfollowAsync(1, 2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetFollowersAsync_ReturnsUsersFollowingSpecifiedUser()
    {
        // Arrange
        var userA = new User { Username = "userA", Email = "a@example.com", PasswordHash = "a" };
        var userB = new User { Username = "userB", Email = "b@example.com", PasswordHash = "b" };
        var userC = new User { Username = "userC", Email = "c@example.com", PasswordHash = "c" };
        _context.Users.AddRange(userA, userB, userC);
        await _context.SaveChangesAsync();

        _context.Follows.AddRange(
            new Follow { FollowerId = userB.Id, FollowingId = userA.Id, CreatedAt = DateTime.UtcNow },
            new Follow { FollowerId = userC.Id, FollowingId = userA.Id, CreatedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetFollowersAsync(userA.Id);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, u => u.Id == userB.Id);
        Assert.Contains(result, u => u.Id == userC.Id);
    }

    [Fact]
    public async Task GetFollowingAsync_ReturnsUsersFollowedBySpecifiedUser()
    {
        // Arrange
        var userA = new User { Username = "userA", Email = "a@example.com", PasswordHash = "a" };
        var userB = new User { Username = "userB", Email = "b@example.com", PasswordHash = "b" };
        var userC = new User { Username = "userC", Email = "c@example.com", PasswordHash = "c" };
        _context.Users.AddRange(userA, userB, userC);
        await _context.SaveChangesAsync();

        _context.Follows.AddRange(
            new Follow { FollowerId = userA.Id, FollowingId = userB.Id, CreatedAt = DateTime.UtcNow },
            new Follow { FollowerId = userA.Id, FollowingId = userC.Id, CreatedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetFollowingAsync(userA.Id);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, u => u.Id == userB.Id);
        Assert.Contains(result, u => u.Id == userC.Id);
    }

    [Fact]
    public async Task GetFollowersCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var userA = new User { Username = "userA", Email = "a@example.com", PasswordHash = "a" };
        var userB = new User { Username = "userB", Email = "b@example.com", PasswordHash = "b" };
        _context.Users.AddRange(userA, userB);
        await _context.SaveChangesAsync();

        _context.Follows.Add(new Follow { FollowerId = userB.Id, FollowingId = userA.Id, CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetFollowersCountAsync(userA.Id);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task GetFollowingCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var userA = new User { Username = "userA", Email = "a@example.com", PasswordHash = "a" };
        var userB = new User { Username = "userB", Email = "b@example.com", PasswordHash = "b" };
        _context.Users.AddRange(userA, userB);
        await _context.SaveChangesAsync();

        _context.Follows.Add(new Follow { FollowerId = userA.Id, FollowingId = userB.Id, CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetFollowingCountAsync(userA.Id);

        // Assert
        Assert.Equal(1, result);
    }
}

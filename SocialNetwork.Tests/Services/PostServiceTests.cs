using Microsoft.EntityFrameworkCore;
using SocialNetwork.API.Data;
using SocialNetwork.API.DTOs;
using SocialNetwork.API.Models;
using SocialNetwork.API.Services;

namespace SocialNetwork.Tests.Services;

public class PostServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly PostService _service;

    public PostServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _service = new PostService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var user = new User { Username = "author", Email = "a@example.com", PasswordHash = "hash" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new CreatePostDto { Content = "Test post", TimelineOwnerId = null };

        // Act
        var result = await _service.CreateAsync(dto, user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Test post", result.Value!.Content);
        Assert.Equal(user.Id, result.Value.AuthorId);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyContent_ReturnsBadRequest()
    {
        // Arrange
        var user = new User { Username = "author", Email = "a@example.com", PasswordHash = "hash" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new CreatePostDto { Content = "", TimelineOwnerId = null };

        // Act
        var result = await _service.CreateAsync(dto, user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultStatus.BadRequest, result.Status);
    }

    [Fact]
    public async Task CreateAsync_WithContentOver500Chars_ReturnsBadRequest()
    {
        // Arrange
        var user = new User { Username = "author", Email = "a@example.com", PasswordHash = "hash" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new CreatePostDto { Content = new string('x', 501), TimelineOwnerId = null };

        // Act
        var result = await _service.CreateAsync(dto, user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultStatus.BadRequest, result.Status);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidAuthor_ReturnsNotFound()
    {
        // Arrange
        var dto = new CreatePostDto { Content = "Test post", TimelineOwnerId = null };

        // Act
        var result = await _service.CreateAsync(dto, 999);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidTimelineOwner_ReturnsNotFound()
    {
        // Arrange
        var user = new User { Username = "author", Email = "a@example.com", PasswordHash = "hash" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new CreatePostDto { Content = "Test post", TimelineOwnerId = 999 };

        // Act
        var result = await _service.CreateAsync(dto, user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task GetWallAsync_ReturnsOwnAndFollowedUsersPosts()
    {
        // Arrange
        var userA = new User { Username = "userA", Email = "a@example.com", PasswordHash = "a" };
        var userB = new User { Username = "userB", Email = "b@example.com", PasswordHash = "b" };
        var userC = new User { Username = "userC", Email = "c@example.com", PasswordHash = "c" };
        _context.Users.AddRange(userA, userB, userC);
        await _context.SaveChangesAsync();

        _context.Follows.Add(new Follow { FollowerId = userA.Id, FollowingId = userB.Id, CreatedAt = DateTime.UtcNow });
        _context.Posts.AddRange(
            new Post { Content = "Post by A", AuthorId = userA.Id, CreatedAt = DateTime.UtcNow.AddMinutes(-5) },
            new Post { Content = "Post by B", AuthorId = userB.Id, CreatedAt = DateTime.UtcNow.AddMinutes(-4) },
            new Post { Content = "Post by C", AuthorId = userC.Id, CreatedAt = DateTime.UtcNow.AddMinutes(-3) }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetWallAsync(userA.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.Contains(result.Value, p => p.AuthorId == userA.Id);
        Assert.Contains(result.Value, p => p.AuthorId == userB.Id);
        Assert.DoesNotContain(result.Value, p => p.AuthorId == userC.Id);
    }

    [Fact]
    public async Task GetWallAsync_WithInvalidUser_ReturnsNotFound()
    {
        // Act
        var result = await _service.GetWallAsync(999);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task GetWallAsync_ReturnsPostsInChronologicalOrder()
    {
        // Arrange
        var user = new User { Username = "user", Email = "u@example.com", PasswordHash = "hash" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _context.Posts.AddRange(
            new Post { Content = "Old post", AuthorId = user.Id, CreatedAt = DateTime.UtcNow.AddHours(-2) },
            new Post { Content = "New post", AuthorId = user.Id, CreatedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetWallAsync(user.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("New post", result.Value![0].Content);
        Assert.Equal("Old post", result.Value[1].Content);
    }

    [Fact]
    public async Task GetTimelineAsync_ReturnsPostsOnUserTimeline()
    {
        // Arrange
        var userA = new User { Username = "userA", Email = "a@example.com", PasswordHash = "a" };
        var userB = new User { Username = "userB", Email = "b@example.com", PasswordHash = "b" };
        _context.Users.AddRange(userA, userB);
        await _context.SaveChangesAsync();

        _context.Posts.AddRange(
            new Post { Content = "Post by A", AuthorId = userA.Id, TimelineOwnerId = null, CreatedAt = DateTime.UtcNow },
            new Post { Content = "Post on A's timeline", AuthorId = userB.Id, TimelineOwnerId = userA.Id, CreatedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTimelineAsync(userA.Id);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Content == "Post by A");
        Assert.Contains(result, p => p.Content == "Post on A's timeline");
    }

    [Fact]
    public async Task DeleteAsync_WithExistingPost_ReturnsTrue()
    {
        // Arrange
        var user = new User { Username = "user", Email = "u@example.com", PasswordHash = "hash" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var post = new Post { Content = "To delete", AuthorId = user.Id, CreatedAt = DateTime.UtcNow };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteAsync(post.Id);

        // Assert
        Assert.True(result);
        Assert.Null(await _context.Posts.FindAsync(post.Id));
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingPost_ReturnsFalse()
    {
        // Act
        var result = await _service.DeleteAsync(999);

        // Assert
        Assert.False(result);
    }
}

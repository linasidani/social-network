using Microsoft.EntityFrameworkCore;
using SocialNetwork.API.Data;
using SocialNetwork.API.DTOs;
using SocialNetwork.API.Models;
using SocialNetwork.API.Services;

namespace SocialNetwork.Tests.Services;

public class DirectMessageServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly DirectMessageService _service;

    public DirectMessageServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _service = new DirectMessageService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task SendAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var sender = new User { Username = "sender", Email = "s@example.com", PasswordHash = "hash" };
        var receiver = new User { Username = "receiver", Email = "r@example.com", PasswordHash = "hash" };
        _context.Users.AddRange(sender, receiver);
        await _context.SaveChangesAsync();

        var dto = new CreateDirectMessageDto { Content = "Hello!", ReceiverId = receiver.Id };

        // Act
        var result = await _service.SendAsync(dto, sender.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Hello!", result.Value!.Content);
        Assert.Equal(sender.Id, result.Value.SenderId);
        Assert.Equal(receiver.Id, result.Value.ReceiverId);
    }

    [Fact]
    public async Task SendAsync_WithEmptyContent_ReturnsBadRequest()
    {
        // Arrange
        var sender = new User { Username = "sender", Email = "s@example.com", PasswordHash = "hash" };
        var receiver = new User { Username = "receiver", Email = "r@example.com", PasswordHash = "hash" };
        _context.Users.AddRange(sender, receiver);
        await _context.SaveChangesAsync();

        var dto = new CreateDirectMessageDto { Content = "", ReceiverId = receiver.Id };

        // Act
        var result = await _service.SendAsync(dto, sender.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultStatus.BadRequest, result.Status);
    }

    [Fact]
    public async Task SendAsync_WithWhitespaceContent_ReturnsBadRequest()
    {
        // Arrange
        var sender = new User { Username = "sender", Email = "s@example.com", PasswordHash = "hash" };
        var receiver = new User { Username = "receiver", Email = "r@example.com", PasswordHash = "hash" };
        _context.Users.AddRange(sender, receiver);
        await _context.SaveChangesAsync();

        var dto = new CreateDirectMessageDto { Content = "   ", ReceiverId = receiver.Id };

        // Act
        var result = await _service.SendAsync(dto, sender.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultStatus.BadRequest, result.Status);
    }

    [Fact]
    public async Task SendAsync_WhenSenderIsReceiver_ReturnsBadRequest()
    {
        // Arrange
        var user = new User { Username = "user", Email = "u@example.com", PasswordHash = "hash" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new CreateDirectMessageDto { Content = "Hello!", ReceiverId = user.Id };

        // Act
        var result = await _service.SendAsync(dto, user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultStatus.BadRequest, result.Status);
    }

    [Fact]
    public async Task SendAsync_WithInvalidSender_ReturnsNotFound()
    {
        // Arrange
        var receiver = new User { Username = "receiver", Email = "r@example.com", PasswordHash = "hash" };
        _context.Users.Add(receiver);
        await _context.SaveChangesAsync();

        var dto = new CreateDirectMessageDto { Content = "Hello!", ReceiverId = receiver.Id };

        // Act
        var result = await _service.SendAsync(dto, 999);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task SendAsync_WithInvalidReceiver_ReturnsNotFound()
    {
        // Arrange
        var sender = new User { Username = "sender", Email = "s@example.com", PasswordHash = "hash" };
        _context.Users.Add(sender);
        await _context.SaveChangesAsync();

        var dto = new CreateDirectMessageDto { Content = "Hello!", ReceiverId = 999 };

        // Act
        var result = await _service.SendAsync(dto, sender.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task SendAsync_BothUsersInvalid_ReturnsNotFound()
    {
        // Arrange
        var dto = new CreateDirectMessageDto { Content = "Hello!", ReceiverId = 999 };

        // Act
        var result = await _service.SendAsync(dto, 888);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task GetConversationAsync_ReturnsMessagesBetweenTwoUsers()
    {
        // Arrange
        var userA = new User { Username = "userA", Email = "a@example.com", PasswordHash = "a" };
        var userB = new User { Username = "userB", Email = "b@example.com", PasswordHash = "b" };
        _context.Users.AddRange(userA, userB);
        await _context.SaveChangesAsync();

        _context.DirectMessages.AddRange(
            new DirectMessage { Content = "A to B", SenderId = userA.Id, ReceiverId = userB.Id, CreatedAt = DateTime.UtcNow.AddMinutes(-2) },
            new DirectMessage { Content = "B to A", SenderId = userB.Id, ReceiverId = userA.Id, CreatedAt = DateTime.UtcNow.AddMinutes(-1) },
            new DirectMessage { Content = "A to B again", SenderId = userA.Id, ReceiverId = userB.Id, CreatedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetConversationAsync(userA.Id, userB.Id);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, m => m.Content == "A to B");
        Assert.Contains(result, m => m.Content == "B to A");
        Assert.Contains(result, m => m.Content == "A to B again");
    }

    [Fact]
    public async Task GetConversationAsync_ReturnsMessagesInChronologicalOrder()
    {
        // Arrange
        var userA = new User { Username = "userA", Email = "a@example.com", PasswordHash = "a" };
        var userB = new User { Username = "userB", Email = "b@example.com", PasswordHash = "b" };
        _context.Users.AddRange(userA, userB);
        await _context.SaveChangesAsync();

        _context.DirectMessages.AddRange(
            new DirectMessage { Content = "Old", SenderId = userA.Id, ReceiverId = userB.Id, CreatedAt = DateTime.UtcNow.AddHours(-2) },
            new DirectMessage { Content = "New", SenderId = userB.Id, ReceiverId = userA.Id, CreatedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetConversationAsync(userA.Id, userB.Id);

        // Assert
        Assert.Equal("Old", result[0].Content);
        Assert.Equal("New", result[1].Content);
    }

    [Fact]
    public async Task GetInboxAsync_ReturnsOnlyReceivedMessages()
    {
        // Arrange
        var userA = new User { Username = "userA", Email = "a@example.com", PasswordHash = "a" };
        var userB = new User { Username = "userB", Email = "b@example.com", PasswordHash = "b" };
        _context.Users.AddRange(userA, userB);
        await _context.SaveChangesAsync();

        _context.DirectMessages.AddRange(
            new DirectMessage { Content = "A to B", SenderId = userA.Id, ReceiverId = userB.Id, CreatedAt = DateTime.UtcNow.AddMinutes(-1) },
            new DirectMessage { Content = "B to A", SenderId = userB.Id, ReceiverId = userA.Id, CreatedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetInboxAsync(userA.Id);

        // Assert
        Assert.Single(result);
        Assert.Equal("B to A", result[0].Content);
    }

    [Fact]
    public async Task GetInboxAsync_ReturnsMessagesInDescendingOrder()
    {
        // Arrange
        var userA = new User { Username = "userA", Email = "a@example.com", PasswordHash = "a" };
        var userB = new User { Username = "userB", Email = "b@example.com", PasswordHash = "b" };
        _context.Users.AddRange(userA, userB);
        await _context.SaveChangesAsync();

        _context.DirectMessages.AddRange(
            new DirectMessage { Content = "Old", SenderId = userB.Id, ReceiverId = userA.Id, CreatedAt = DateTime.UtcNow.AddHours(-2) },
            new DirectMessage { Content = "New", SenderId = userB.Id, ReceiverId = userA.Id, CreatedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetInboxAsync(userA.Id);

        // Assert
        Assert.Equal("New", result[0].Content);
        Assert.Equal("Old", result[1].Content);
    }

    [Fact]
    public async Task GetSentAsync_ReturnsOnlySentMessages()
    {
        // Arrange
        var userA = new User { Username = "userA", Email = "a@example.com", PasswordHash = "a" };
        var userB = new User { Username = "userB", Email = "b@example.com", PasswordHash = "b" };
        _context.Users.AddRange(userA, userB);
        await _context.SaveChangesAsync();

        _context.DirectMessages.AddRange(
            new DirectMessage { Content = "A to B", SenderId = userA.Id, ReceiverId = userB.Id, CreatedAt = DateTime.UtcNow.AddMinutes(-1) },
            new DirectMessage { Content = "B to A", SenderId = userB.Id, ReceiverId = userA.Id, CreatedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetSentAsync(userA.Id);

        // Assert
        Assert.Single(result);
        Assert.Equal("A to B", result[0].Content);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingMessage_ReturnsTrue()
    {
        // Arrange
        var sender = new User { Username = "sender", Email = "s@example.com", PasswordHash = "hash" };
        var receiver = new User { Username = "receiver", Email = "r@example.com", PasswordHash = "hash" };
        _context.Users.AddRange(sender, receiver);
        await _context.SaveChangesAsync();

        var message = new DirectMessage { Content = "To delete", SenderId = sender.Id, ReceiverId = receiver.Id, CreatedAt = DateTime.UtcNow };
        _context.DirectMessages.Add(message);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteAsync(message.Id);

        // Assert
        Assert.True(result);
        Assert.Null(await _context.DirectMessages.FindAsync(message.Id));
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingMessage_ReturnsFalse()
    {
        // Act
        var result = await _service.DeleteAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DirectMessages_NotVisibleInPublicFeeds()
    {
        // Arrange - Verify DM exists but is not in posts
        var userA = new User { Username = "userA", Email = "a@example.com", PasswordHash = "a" };
        var userB = new User { Username = "userB", Email = "b@example.com", PasswordHash = "b" };
        _context.Users.AddRange(userA, userB);
        await _context.SaveChangesAsync();

        _context.DirectMessages.Add(new DirectMessage { Content = "Secret DM", SenderId = userA.Id, ReceiverId = userB.Id, CreatedAt = DateTime.UtcNow });
        _context.Posts.Add(new Post { Content = "Public post", AuthorId = userA.Id, CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        // Act & Assert
        var dmInDb = await _context.DirectMessages.FirstOrDefaultAsync();
        var posts = await _context.Posts.ToListAsync();

        Assert.NotNull(dmInDb);
        Assert.Single(posts);
        Assert.Equal("Public post", posts[0].Content);
        Assert.DoesNotContain(posts, p => p.Content == "Secret DM");
    }
}

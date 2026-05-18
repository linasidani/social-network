using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SocialNetwork.API.Data;
using SocialNetwork.API.Controllers;
using SocialNetwork.API.Models;
using Microsoft.Extensions.Logging;
using Moq;
using SocialNetwork.API.Services;

namespace SocialNetwork.Tests;

public class BasicTests : IDisposable
{
    private AppDbContext _context;
    private Mock<ILogger<UsersController>> _mockLogger;
    private UsersController _controller;

    public BasicTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _mockLogger = new Mock<ILogger<UsersController>>();
        _controller = new UsersController(_context, _mockLogger.Object, CreateAuthService());
    }

    private static AuthService CreateAuthService()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-secret-key-for-auth-service-123456789",
                ["Jwt:Issuer"] = "SocialNetwork.API.Tests",
                ["Jwt:Audience"] = "SocialNetwork.Tests",
                ["Jwt:ExpiresInMinutes"] = "60"
            })
            .Build();

        return new AuthService(configuration);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    [Fact]
    public void UserControllerExists()
    {
        Assert.NotNull(_controller);
    }

    [Fact]
    public async Task CanCreateUser()
    {
        var user = new User 
        { 
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Username == "testuser");
        Assert.NotNull(userInDb);
        Assert.Equal("testuser", userInDb.Username);
    }

    [Fact]
    public async Task CanCreatePost()
    {
        var user = new User 
        { 
            Username = "postuser",
            Email = "post@example.com",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var post = new Post
        {
            Content = "Test post content",
            AuthorId = user.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        var postInDb = await _context.Posts.FirstOrDefaultAsync(p => p.Content == "Test post content");
        Assert.NotNull(postInDb);
        Assert.Equal(user.Id, postInDb.AuthorId);
    }

    [Fact]
    public async Task CanFollowUser()
    {
        var user1 = new User 
        { 
            Username = "user1",
            Email = "user1@example.com",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow
        };
        var user2 = new User 
        { 
            Username = "user2",
            Email = "user2@example.com",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.AddRange(user1, user2);
        await _context.SaveChangesAsync();

        var follow = new Follow
        {
            FollowerId = user1.Id,
            FollowingId = user2.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.Follows.Add(follow);
        await _context.SaveChangesAsync();

        var followInDb = await _context.Follows.FirstOrDefaultAsync(f => f.FollowerId == user1.Id);
        Assert.NotNull(followInDb);
        Assert.Equal(user2.Id, followInDb.FollowingId);
    }

    [Fact]
    public async Task CanSendDirectMessage()
    {
        var user1 = new User 
        { 
            Username = "sender",
            Email = "sender@example.com",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow
        };
        var user2 = new User 
        { 
            Username = "receiver",
            Email = "receiver@example.com",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.AddRange(user1, user2);
        await _context.SaveChangesAsync();

        var message = new DirectMessage
        {
            Content = "Hello!",
            SenderId = user1.Id,
            ReceiverId = user2.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.DirectMessages.Add(message);
        await _context.SaveChangesAsync();

        var messageInDb = await _context.DirectMessages.FirstOrDefaultAsync(m => m.Content == "Hello!");
        Assert.NotNull(messageInDb);
        Assert.Equal(user1.Id, messageInDb.SenderId);
    }
}

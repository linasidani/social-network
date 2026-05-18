using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SocialNetwork.API.Controllers;
using SocialNetwork.API.Data;
using SocialNetwork.API.DTOs;
using SocialNetwork.API.Models;

namespace SocialNetwork.Tests;

public class ControllerTests
{
    private static AppDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task RegisterUser_CreatesUserAndReturnsCreatedResult()
    {
        using var context = CreateContext(nameof(RegisterUser_CreatesUserAndReturnsCreatedResult));
        var controller = new UsersController(context, new LoggerFactory().CreateLogger<UsersController>());

        var dto = new RegisterUserDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "password123"
        };

        var result = await controller.Register(dto);
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var userDto = Assert.IsType<UserDto>(createdResult.Value);

        Assert.Equal(dto.Username, userDto.Username);
        Assert.Equal(dto.Email, userDto.Email);
        Assert.True(userDto.Id > 0);
    }

    [Fact]
    public async Task RegisterUser_DuplicateEmail_ReturnsBadRequest()
    {
        using var context = CreateContext(nameof(RegisterUser_DuplicateEmail_ReturnsBadRequest));
        context.Users.Add(new User { Username = "existing", Email = "dup@example.com", PasswordHash = "abc" });
        await context.SaveChangesAsync();

        var controller = new UsersController(context, new LoggerFactory().CreateLogger<UsersController>());
        var dto = new RegisterUserDto
        {
            Username = "newuser",
            Email = "dup@example.com",
            Password = "password123"
        };

        var result = await controller.Register(dto);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Username or email already exists", badRequest.Value);
    }

    [Fact]
    public async Task GetAllUsers_ReturnsAllRegisteredUsers()
    {
        using var context = CreateContext(nameof(GetAllUsers_ReturnsAllRegisteredUsers));
        context.Users.AddRange(
            new User { Username = "user1", Email = "user1@example.com", PasswordHash = "a" },
            new User { Username = "user2", Email = "user2@example.com", PasswordHash = "b" }
        );
        await context.SaveChangesAsync();

        var controller = new UsersController(context, new LoggerFactory().CreateLogger<UsersController>());
        var result = await controller.GetAllUsers();
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var users = Assert.IsType<List<UserDto>>(okResult.Value!);

        Assert.Equal(2, users.Count);
    }

    [Fact]
    public async Task CreatePost_WithValidAuthor_ReturnsCreatedPost()
    {
        using var context = CreateContext(nameof(CreatePost_WithValidAuthor_ReturnsCreatedPost));
        var user = new User { Username = "author", Email = "author@example.com", PasswordHash = "abc" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = new PostsController(context, new LoggerFactory().CreateLogger<PostsController>());
        var dto = new CreatePostDto { Content = "Hello world", TimelineOwnerId = null };

        var result = await controller.CreatePost(dto, user.Id);
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var postDto = Assert.IsType<PostDto>(createdResult.Value);

        Assert.Equal(dto.Content, postDto.Content);
        Assert.Equal(user.Id, postDto.AuthorId);
        Assert.Equal(user.Username, postDto.AuthorUsername);
    }

    [Fact]
    public async Task CreatePost_WithInvalidTimelineOwner_ReturnsNotFound()
    {
        using var context = CreateContext(nameof(CreatePost_WithInvalidTimelineOwner_ReturnsNotFound));
        var user = new User { Username = "author", Email = "author@example.com", PasswordHash = "abc" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = new PostsController(context, new LoggerFactory().CreateLogger<PostsController>());
        var dto = new CreatePostDto { Content = "Message to nobody", TimelineOwnerId = 999 };

        var result = await controller.CreatePost(dto, user.Id);
        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Timeline owner not found", notFound.Value);
    }

    [Fact]
    public async Task FollowUser_CreatesFollowRelation()
    {
        using var context = CreateContext(nameof(FollowUser_CreatesFollowRelation));
        var follower = new User { Username = "follower", Email = "follower@example.com", PasswordHash = "abc" };
        var following = new User { Username = "following", Email = "following@example.com", PasswordHash = "abc" };
        context.Users.AddRange(follower, following);
        await context.SaveChangesAsync();

        var controller = new FollowsController(context, new LoggerFactory().CreateLogger<FollowsController>());
        var result = await controller.FollowUser(follower.Id, following.Id);
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var followDto = Assert.IsType<FollowDto>(createdResult.Value);

        Assert.Equal(follower.Id, followDto.FollowerId);
        Assert.Equal(following.Id, followDto.FollowingId);
    }

    [Fact]
    public async Task GetWall_IncludesFollowedUsersPosts()
    {
        using var context = CreateContext(nameof(GetWall_IncludesFollowedUsersPosts));
        var userA = new User { Username = "userA", Email = "a@example.com", PasswordHash = "a" };
        var userB = new User { Username = "userB", Email = "b@example.com", PasswordHash = "b" };
        var userC = new User { Username = "userC", Email = "c@example.com", PasswordHash = "c" };
        context.Users.AddRange(userA, userB, userC);
        await context.SaveChangesAsync();

        context.Follows.Add(new Follow { FollowerId = userA.Id, FollowingId = userB.Id, CreatedAt = DateTime.UtcNow });
        context.Posts.AddRange(
            new Post { Content = "Post by A", AuthorId = userA.Id, CreatedAt = DateTime.UtcNow.AddMinutes(-5) },
            new Post { Content = "Post by B", AuthorId = userB.Id, CreatedAt = DateTime.UtcNow.AddMinutes(-4) },
            new Post { Content = "Post by C", AuthorId = userC.Id, CreatedAt = DateTime.UtcNow.AddMinutes(-3) }
        );
        await context.SaveChangesAsync();

        var controller = new PostsController(context, new LoggerFactory().CreateLogger<PostsController>());
        var result = await controller.GetWall(userA.Id);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var posts = Assert.IsType<List<PostDto>>(okResult.Value!);

        Assert.Contains(posts, p => p.AuthorId == userA.Id);
        Assert.Contains(posts, p => p.AuthorId == userB.Id);
        Assert.DoesNotContain(posts, p => p.AuthorId == userC.Id);
        Assert.Equal(2, posts.Count);
    }

    [Fact]
    public async Task SendMessage_AndInbox_ReturnsReceivedMessage()
    {
        using var context = CreateContext(nameof(SendMessage_AndInbox_ReturnsReceivedMessage));
        var sender = new User { Username = "sender", Email = "sender@example.com", PasswordHash = "abc" };
        var receiver = new User { Username = "receiver", Email = "receiver@example.com", PasswordHash = "abc" };
        context.Users.AddRange(sender, receiver);
        await context.SaveChangesAsync();

        var controller = new DirectMessagesController(context, new LoggerFactory().CreateLogger<DirectMessagesController>());
        var sendResult = await controller.SendMessage(new CreateDirectMessageDto { Content = "Hello", ReceiverId = receiver.Id }, sender.Id);
        var createdResult = Assert.IsType<CreatedAtActionResult>(sendResult.Result);
        var messageDto = Assert.IsType<DirectMessageDto>(createdResult.Value);

        Assert.Equal(sender.Id, messageDto.SenderId);
        Assert.Equal(receiver.Id, messageDto.ReceiverId);
        Assert.Equal("Hello", messageDto.Content);

        var inboxResult = await controller.GetInbox(receiver.Id);
        var okInbox = Assert.IsType<OkObjectResult>(inboxResult.Result);
        var inboxMessages = Assert.IsType<List<DirectMessageDto>>(okInbox.Value!);

        Assert.Single(inboxMessages);
        Assert.Equal("Hello", inboxMessages[0].Content);
    }
}

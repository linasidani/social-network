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
}

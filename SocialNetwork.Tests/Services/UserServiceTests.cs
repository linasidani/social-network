using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SocialNetwork.API.Data;
using SocialNetwork.API.DTOs;
using SocialNetwork.API.Models;
using SocialNetwork.API.Services;

namespace SocialNetwork.Tests.Services;

public class UserServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UserService _service;

    public UserServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _service = new UserService(_context, CreateAuthService());
    }

    public void Dispose()
    {
        _context.Dispose();
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

    [Fact]
    public async Task RegisterAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var dto = new RegisterUserDto { Username = "newuser", Email = "new@example.com", Password = "password123" };

        // Act
        var result = await _service.RegisterAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("newuser", result.Value!.Username);
        Assert.Equal("new@example.com", result.Value.Email);
        Assert.True(result.Value.Id > 0);
    }

    [Fact]
    public async Task RegisterAsync_WithEmptyUsername_ReturnsBadRequest()
    {
        // Arrange
        var dto = new RegisterUserDto { Username = "", Email = "test@example.com", Password = "password123" };

        // Act
        var result = await _service.RegisterAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultStatus.BadRequest, result.Status);
    }

    [Fact]
    public async Task RegisterAsync_WithEmptyEmail_ReturnsBadRequest()
    {
        // Arrange
        var dto = new RegisterUserDto { Username = "user", Email = "", Password = "password123" };

        // Act
        var result = await _service.RegisterAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultStatus.BadRequest, result.Status);
    }

    [Fact]
    public async Task RegisterAsync_WithEmptyPassword_ReturnsBadRequest()
    {
        // Arrange
        var dto = new RegisterUserDto { Username = "user", Email = "test@example.com", Password = "" };

        // Act
        var result = await _service.RegisterAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultStatus.BadRequest, result.Status);
    }

    [Fact]
    public async Task RegisterAsync_WithWhitespaceOnly_ReturnsBadRequest()
    {
        // Arrange
        var dto = new RegisterUserDto { Username = "   ", Email = "   ", Password = "   " };

        // Act
        var result = await _service.RegisterAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultStatus.BadRequest, result.Status);
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateUsername_ReturnsBadRequest()
    {
        // Arrange
        var existing = new User { Username = "existing", Email = "e1@example.com", PasswordHash = "hash" };
        _context.Users.Add(existing);
        await _context.SaveChangesAsync();

        var dto = new RegisterUserDto { Username = "existing", Email = "new@example.com", Password = "password123" };

        // Act
        var result = await _service.RegisterAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultStatus.BadRequest, result.Status);
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var existing = new User { Username = "user1", Email = "dup@example.com", PasswordHash = "hash" };
        _context.Users.Add(existing);
        await _context.SaveChangesAsync();

        var dto = new RegisterUserDto { Username = "user2", Email = "dup@example.com", Password = "password123" };

        // Act
        var result = await _service.RegisterAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultStatus.BadRequest, result.Status);
    }

    [Fact]
    public async Task RegisterAsync_PasswordIsHashed_NotStoredPlaintext()
    {
        // Arrange
        var dto = new RegisterUserDto { Username = "user", Email = "test@example.com", Password = "password123" };

        // Act
        await _service.RegisterAsync(dto);
        var userInDb = await _context.Users.FirstAsync();

        // Assert
        Assert.NotEqual("password123", userInDb.PasswordHash);
        Assert.StartsWith("PBKDF2$", userInDb.PasswordHash);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccessWithToken()
    {
        // Arrange
        var authService = CreateAuthService();
        var passwordHash = authService.HashPassword("password123");
        var user = new User { Username = "testuser", Email = "test@example.com", PasswordHash = passwordHash };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new LoginUserDto { UsernameOrEmail = "testuser", Password = "password123" };

        // Act
        var result = await _service.LoginAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value!.Token);
        Assert.NotNull(result.Value.User);
        Assert.Equal("testuser", result.Value.User.Username);
    }

    [Fact]
    public async Task LoginAsync_WithValidEmail_ReturnsSuccess()
    {
        // Arrange
        var authService = CreateAuthService();
        var passwordHash = authService.HashPassword("password123");
        var user = new User { Username = "testuser", Email = "test@example.com", PasswordHash = passwordHash };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new LoginUserDto { UsernameOrEmail = "test@example.com", Password = "password123" };

        // Act
        var result = await _service.LoginAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ReturnsUnauthorized()
    {
        // Arrange
        var authService = CreateAuthService();
        var passwordHash = authService.HashPassword("password123");
        var user = new User { Username = "testuser", Email = "test@example.com", PasswordHash = passwordHash };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new LoginUserDto { UsernameOrEmail = "testuser", Password = "wrongpassword" };

        // Act
        var result = await _service.LoginAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultStatus.Unauthorized, result.Status);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistingUser_ReturnsUnauthorized()
    {
        // Arrange
        var dto = new LoginUserDto { UsernameOrEmail = "nonexistent", Password = "password123" };

        // Act
        var result = await _service.LoginAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultStatus.Unauthorized, result.Status);
    }

    [Fact]
    public async Task LoginAsync_WithEmptyUsernameOrEmail_ReturnsBadRequest()
    {
        // Arrange
        var dto = new LoginUserDto { UsernameOrEmail = "", Password = "password123" };

        // Act
        var result = await _service.LoginAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultStatus.BadRequest, result.Status);
    }

    [Fact]
    public async Task LoginAsync_WithEmptyPassword_ReturnsBadRequest()
    {
        // Arrange
        var dto = new LoginUserDto { UsernameOrEmail = "user", Password = "" };

        // Act
        var result = await _service.LoginAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceResultStatus.BadRequest, result.Status);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        // Arrange
        _context.Users.AddRange(
            new User { Username = "user1", Email = "u1@example.com", PasswordHash = "a" },
            new User { Username = "user2", Email = "u2@example.com", PasswordHash = "b" }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingUser_ReturnsUser()
    {
        // Arrange
        var user = new User { Username = "user", Email = "u@example.com", PasswordHash = "hash" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("user", result!.Username);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingUser_ReturnsNull()
    {
        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUsernameAsync_WithExistingUser_ReturnsUser()
    {
        // Arrange
        var user = new User { Username = "testuser", Email = "u@example.com", PasswordHash = "hash" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByUsernameAsync("testuser");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("testuser", result!.Username);
    }

    [Fact]
    public async Task GetByUsernameAsync_WithNonExistingUser_ReturnsNull()
    {
        // Act
        var result = await _service.GetByUsernameAsync("nonexistent");

        // Assert
        Assert.Null(result);
    }
}

using Microsoft.Extensions.Configuration;
using SocialNetwork.API.Models;
using SocialNetwork.API.Services;
using System.IdentityModel.Tokens.Jwt;

namespace SocialNetwork.Tests.Services;

public class AuthServiceTests
{
    private readonly AuthService _service;

    public AuthServiceTests()
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

        _service = new AuthService(configuration);
    }

    [Fact]
    public void HashPassword_ReturnsNonEmptyString()
    {
        // Act
        var hash = _service.HashPassword("password123");

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.StartsWith("PBKDF2$", hash);
    }

    [Fact]
    public void HashPassword_SamePasswordDifferentHashes()
    {
        // Act
        var hash1 = _service.HashPassword("password123");
        var hash2 = _service.HashPassword("password123");

        // Assert - Different salts produce different hashes
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var hash = _service.HashPassword("password123");

        // Act
        var result = _service.VerifyPassword("password123", hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var hash = _service.HashPassword("password123");

        // Act
        var result = _service.VerifyPassword("wrongpassword", hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_WithInvalidHashFormat_ReturnsFalse()
    {
        // Act
        var result = _service.VerifyPassword("password123", "invalid-hash");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_WithEmptyHash_ReturnsFalse()
    {
        // Act
        var result = _service.VerifyPassword("password123", "");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CreateToken_ReturnsValidTokenAndExpiration()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com"
        };

        // Act
        var (token, expiresAt) = _service.CreateToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.True(expiresAt > DateTime.UtcNow);
        Assert.True(expiresAt < DateTime.UtcNow.AddHours(2));
    }

    [Fact]
    public void CreateToken_ContainsValidJwtStructure()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com"
        };

        // Act
        var (token, _) = _service.CreateToken(user);
        var parts = token.Split('.');

        // Assert - JWT has 3 parts: header.payload.signature
        Assert.Equal(3, parts.Length);
    }

    [Fact]
    public void CreateToken_CanBeValidated()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com"
        };

        // Act
        var (token, _) = _service.CreateToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert - check claims using the standard JWT claim names
        Assert.Contains(jwtToken.Claims, c => c.Value == "testuser");
        Assert.Contains(jwtToken.Claims, c => c.Value == "1");
        Assert.Contains(jwtToken.Claims, c => c.Value == "test@example.com");
    }

    [Fact]
    public void VerifyPassword_WithTamperedHash_ReturnsFalse()
    {
        // Arrange
        var hash = _service.HashPassword("password123");
        var tamperedHash = hash.Replace("PBKDF2", "HMACSHA256");

        // Act
        var result = _service.VerifyPassword("password123", tamperedHash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HashPassword_WithEmptyPassword_ReturnsValidHash()
    {
        // Act
        var hash = _service.HashPassword("");

        // Assert
        Assert.NotNull(hash);
        Assert.StartsWith("PBKDF2$", hash);

        // Empty password should still be verifiable
        var verified = _service.VerifyPassword("", hash);
        Assert.True(verified);
    }

    [Fact]
    public void HashPassword_WithLongPassword_WorksCorrectly()
    {
        // Arrange
        var longPassword = new string('a', 1000);

        // Act
        var hash = _service.HashPassword(longPassword);
        var verified = _service.VerifyPassword(longPassword, hash);

        // Assert
        Assert.True(verified);
    }
}

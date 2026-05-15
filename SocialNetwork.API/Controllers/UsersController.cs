using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialNetwork.API.Data;
using SocialNetwork.API.DTOs;
using SocialNetwork.API.Models;
using System.Security.Cryptography;
using System.Text;

namespace SocialNetwork.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<UsersController> _logger;

    public UsersController(AppDbContext context, ILogger<UsersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterUserDto dto)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest("Username, email, and password are required");
        }

        // Check if user already exists
        if (await _context.Users.AnyAsync(u => u.Username == dto.Username || u.Email == dto.Email))
        {
            return BadRequest("Username or email already exists");
        }

        // Hash password
        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = HashPassword(dto.Password),
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"User registered: {user.Username}");

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, MapToDto(user));
    }

    /// <summary>
    /// Get all users
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAllUsers()
    {
        var users = await _context.Users.ToListAsync();
        return Ok(users.Select(MapToDto).ToList());
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(user));
    }

    /// <summary>
    /// Get user by username
    /// </summary>
    [HttpGet("username/{username}")]
    public async Task<ActionResult<UserDto>> GetUserByUsername(string username)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(user));
    }

    private UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        };
    }

    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}

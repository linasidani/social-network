using Microsoft.EntityFrameworkCore;
using SocialNetwork.API.Data;
using SocialNetwork.API.DTOs;
using SocialNetwork.API.Models;

namespace SocialNetwork.API.Services;

public class UserService
{
    private readonly AppDbContext _context;
    private readonly AuthService _authService;

    public UserService(AppDbContext context, AuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    public async Task<ServiceResult<UserDto>> RegisterAsync(RegisterUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) ||
            string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Password))
        {
            return ServiceResult<UserDto>.BadRequest("Username, email, and password are required");
        }

        if (await _context.Users.AnyAsync(u => u.Username == dto.Username || u.Email == dto.Email))
        {
            return ServiceResult<UserDto>.BadRequest("Username or email already exists");
        }

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = _authService.HashPassword(dto.Password),
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return ServiceResult<UserDto>.Success(MapToDto(user));
    }

    public async Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.UsernameOrEmail) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return ServiceResult<AuthResponseDto>.BadRequest("Username/email and password are required");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u =>
            u.Username == dto.UsernameOrEmail || u.Email == dto.UsernameOrEmail);

        if (user == null || !_authService.VerifyPassword(dto.Password, user.PasswordHash))
        {
            return ServiceResult<AuthResponseDto>.Unauthorized("Invalid username/email or password");
        }

        var token = _authService.CreateToken(user);
        return ServiceResult<AuthResponseDto>.Success(new AuthResponseDto
        {
            Token = token.Token,
            ExpiresAt = token.ExpiresAt,
            User = MapToDto(user)
        });
    }

    public async Task<List<UserDto>> GetAllAsync()
    {
        var users = await _context.Users.ToListAsync();
        return users.Select(MapToDto).ToList();
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        return user == null ? null : MapToDto(user);
    }

    public async Task<UserDto?> GetByUsernameAsync(string username)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        return user == null ? null : MapToDto(user);
    }

    public static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        };
    }
}

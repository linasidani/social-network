using Microsoft.EntityFrameworkCore;
using SocialNetwork.API.Data;
using SocialNetwork.API.DTOs;
using SocialNetwork.API.Models;

namespace SocialNetwork.API.Services;

public class PostService
{
    private readonly AppDbContext _context;

    public PostService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResult<PostDto>> CreateAsync(CreatePostDto dto, int authorId)
    {
        if (string.IsNullOrWhiteSpace(dto.Content))
        {
            return ServiceResult<PostDto>.BadRequest("Content is required");
        }

        if (dto.Content.Length > 500)
        {
            return ServiceResult<PostDto>.BadRequest("Content must be 500 characters or fewer");
        }

        var author = await _context.Users.FindAsync(authorId);
        if (author == null)
        {
            return ServiceResult<PostDto>.NotFound("Author not found");
        }

        if (dto.TimelineOwnerId.HasValue)
        {
            var timelineOwner = await _context.Users.FindAsync(dto.TimelineOwnerId.Value);
            if (timelineOwner == null)
            {
                return ServiceResult<PostDto>.NotFound("Timeline owner not found");
            }
        }

        var post = new Post
        {
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow,
            AuthorId = authorId,
            TimelineOwnerId = dto.TimelineOwnerId
        };

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        return ServiceResult<PostDto>.Success(MapToDto(post, author.Username));
    }

    public async Task<ServiceResult<List<PostDto>>> GetWallAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return ServiceResult<List<PostDto>>.NotFound("User not found");
        }

        var followingIds = await _context.Follows
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowingId)
            .ToListAsync();

        var posts = await _context.Posts
            .Include(p => p.Author)
            .Where(p => p.AuthorId == userId || followingIds.Contains(p.AuthorId))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return ServiceResult<List<PostDto>>.Success(posts.Select(p => MapToDto(p, p.Author.Username)).ToList());
    }

    public async Task<PostDto?> GetByIdAsync(int id)
    {
        var post = await _context.Posts.Include(p => p.Author).FirstOrDefaultAsync(p => p.Id == id);
        return post == null ? null : MapToDto(post, post.Author.Username);
    }

    public async Task<List<PostDto>> GetByUserAsync(int userId)
    {
        var posts = await _context.Posts
            .Include(p => p.Author)
            .Where(p => p.AuthorId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return posts.Select(p => MapToDto(p, p.Author.Username)).ToList();
    }

    public async Task<List<PostDto>> GetTimelineAsync(int userId)
    {
        var posts = await _context.Posts
            .Include(p => p.Author)
            .Where(p => p.TimelineOwnerId == userId || p.AuthorId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return posts.Select(p => MapToDto(p, p.Author.Username)).ToList();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post == null)
        {
            return false;
        }

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();

        return true;
    }

    private static PostDto MapToDto(Post post, string authorUsername)
    {
        return new PostDto
        {
            Id = post.Id,
            Content = post.Content,
            CreatedAt = post.CreatedAt,
            AuthorId = post.AuthorId,
            AuthorUsername = authorUsername,
            TimelineOwnerId = post.TimelineOwnerId
        };
    }
}

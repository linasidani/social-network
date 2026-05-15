using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialNetwork.API.Data;
using SocialNetwork.API.DTOs;
using SocialNetwork.API.Models;

namespace SocialNetwork.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<PostsController> _logger;

    public PostsController(AppDbContext context, ILogger<PostsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Create a new post
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PostDto>> CreatePost(CreatePostDto dto, [FromQuery] int authorId)
    {
        if (string.IsNullOrWhiteSpace(dto.Content))
        {
            return BadRequest("Content is required");
        }

        // Verify author exists
        var author = await _context.Users.FindAsync(authorId);
        if (author == null)
        {
            return NotFound("Author not found");
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

        _logger.LogInformation($"Post created by user {authorId}");

        return CreatedAtAction(nameof(GetPost), new { id = post.Id }, MapToDto(post, author.Username));
    }

    /// <summary>
    /// Get post by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PostDto>> GetPost(int id)
    {
        var post = await _context.Posts.Include(p => p.Author).FirstOrDefaultAsync(p => p.Id == id);
        if (post == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(post, post.Author.Username));
    }

    /// <summary>
    /// Get all posts by a user
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<PostDto>>> GetUserPosts(int userId)
    {
        var posts = await _context.Posts
            .Include(p => p.Author)
            .Where(p => p.AuthorId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return Ok(posts.Select(p => MapToDto(p, p.Author.Username)).ToList());
    }

    /// <summary>
    /// Get timeline (posts on a user's profile)
    /// </summary>
    [HttpGet("timeline/{userId}")]
    public async Task<ActionResult<List<PostDto>>> GetTimeline(int userId)
    {
        var posts = await _context.Posts
            .Include(p => p.Author)
            .Where(p => p.TimelineOwnerId == userId || p.AuthorId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return Ok(posts.Select(p => MapToDto(p, p.Author.Username)).ToList());
    }

    /// <summary>
    /// Delete a post
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePost(int id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post == null)
        {
            return NotFound();
        }

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Post {id} deleted");

        return NoContent();
    }

    private PostDto MapToDto(Post post, string authorUsername)
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

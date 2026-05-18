using Microsoft.AspNetCore.Mvc;
using SocialNetwork.API.DTOs;
using SocialNetwork.API.Services;

namespace SocialNetwork.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly ILogger<PostsController> _logger;
    private readonly PostService _postService;

    public PostsController(PostService postService, ILogger<PostsController> logger)
    {
        _logger = logger;
        _postService = postService;
    }

    /// <summary>
    /// Create a new post
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PostDto>> CreatePost(CreatePostDto dto, [FromQuery] int authorId)
    {
        var result = await _postService.CreateAsync(dto, authorId);
        if (!result.IsSuccess)
        {
            return ToActionResult(result);
        }

        _logger.LogInformation("Post created by user {AuthorId}", authorId);

        return CreatedAtAction(nameof(GetPost), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Get wall feed for a user
    /// </summary>
    [HttpGet("wall/{userId}")]
    public async Task<ActionResult<List<PostDto>>> GetWall(int userId)
    {
        var result = await _postService.GetWallAsync(userId);
        return result.IsSuccess ? Ok(result.Value) : ToActionResult(result);
    }

    /// <summary>
    /// Get post by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PostDto>> GetPost(int id)
    {
        var post = await _postService.GetByIdAsync(id);
        if (post == null)
        {
            return NotFound();
        }

        return Ok(post);
    }

    /// <summary>
    /// Get all posts by a user
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<PostDto>>> GetUserPosts(int userId)
    {
        return Ok(await _postService.GetByUserAsync(userId));
    }

    /// <summary>
    /// Get timeline (posts on a user's profile)
    /// </summary>
    [HttpGet("timeline/{userId}")]
    public async Task<ActionResult<List<PostDto>>> GetTimeline(int userId)
    {
        return Ok(await _postService.GetTimelineAsync(userId));
    }

    /// <summary>
    /// Delete a post
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePost(int id)
    {
        if (!await _postService.DeleteAsync(id))
        {
            return NotFound();
        }

        _logger.LogInformation("Post {PostId} deleted", id);

        return NoContent();
    }

    private ActionResult ToActionResult<T>(ServiceResult<T> result)
    {
        return result.Status switch
        {
            ServiceResultStatus.BadRequest => BadRequest(result.Error),
            ServiceResultStatus.NotFound => NotFound(result.Error),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SocialMediaApp.SharedModels;

namespace SocialMediaApp.DataAccessLibrary.Repositories;

public class PostDataAccess : IPostDataAccess
{
    private readonly AppDbContext _context;
    private readonly ILogger<PostDataAccess> _logger;

    public PostDataAccess(AppDbContext context, ILogger<PostDataAccess> logger = null!)
    {
        _context = context;
        _logger = logger ?? NullLogger<PostDataAccess>.Instance;
    }

    public async Task<IEnumerable<Post>> GetPostsAsync()
    {
        try
        {
            return await _context.Posts.Include(post => post.PostVotes).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(2400, ex, "An error occurred while trying to retrieve application's posts. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<IEnumerable<Post>> GetPostsAsync(int amount)
    {
        try
        {
            return await _context.Posts
            .OrderByDescending(post => post.SentAt)
            .Include(post => post.PostVotes)
            .Take(amount)
            .ToListAsync();
        }
        catch (Exception ex)
        {

            _logger.LogError(2401, ex, "An error occurred while trying to retrieve application's posts with Amount:{amount}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", amount, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<IEnumerable<Post>> GetPostsOfUserAsync(string userId)
    {
        try
        {
            return await _context.Posts.Include(post => post.PostVotes).
                Where(post => post.UserId == userId).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(2402, ex, "An error occurred while trying to retrieve users posts with UserId:{UserId}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", userId, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<Post> GetPostAsync(int id)
    {
        try
        {
            return await _context.Posts.Include(post => post.PostVotes).
                FirstOrDefaultAsync(post => post.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(2403, ex, "An error occurred while trying to retrieve post with PostId:{PostId}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<int> CreatePostAsync(Post post)
    {
        try
        {
            var result = await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();

            _logger.LogInformation(0400, "Successfully created post." +
                "PostId:{PostId}, SentAt:{SentAt}, Title:{Title}, Content:{Content}, UserId:{UserId}",
                result.Entity.Id, post.SentAt, post.Title, post.Content, post.UserId);
            return result.Entity.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(2404, ex, "An error occurred while trying to create post. " +
                "SentAt:{SentAt}, Title:{Title}, Content:{Content}, UserId:{UserId}" +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.",
                post.SentAt, post.Title, post.Content, post.UserId, ex.Message, ex.StackTrace);
            return -1;
        }
    }

    public async Task<bool> UpdatePostAsync(int postId, Post post)
    {
        try
        {
            Post foundPost = await GetPostAsync(postId);
            if (foundPost is null)
            {
                _logger.LogWarning(1400, "Attempted to update null post, given postId:{postId}.", postId);
                return false;
            }


            foundPost.SentAt = post.SentAt;
            foundPost.Title = post.Title;
            foundPost.Content = post.Content;
            await _context.SaveChangesAsync();

            _logger.LogInformation(0401, "Successfully updated post." +
                "PostId:{PostId}, SentAt:{SentAt}, Title:{Title}, Content:{Content}, UserId:{UserId}",
                postId, post.SentAt, post.Title, post.Content, post.UserId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(2405, ex, "An error occurred while trying to update post. " +
                "PostId:{PostId}, SentAt:{SentAt}, Title:{Title}, Content:{Content}, UserId:{UserId}" +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.",
                postId, post.SentAt, post.Title, post.Content, post.UserId, ex.Message, ex.StackTrace);
            return false;
        }
    }

    public async Task<bool> DeletePostAsync(int id)
    {
        try
        {
            Post foundPost = await GetPostAsync(id);
            if (foundPost is null)
            {
                _logger.LogWarning(1401, "Attempted to delete null post, given postId:{postId}.", id);
                return false;
            }

            _context.Posts.Remove(foundPost);
            await _context.SaveChangesAsync();

            _logger.LogInformation(0402, "Successfully deleted post with PostId:{PostId}.", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(2406, ex, "An error occurred while trying to delete post with PostId:{PostId}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", id, ex.Message, ex.StackTrace);
            return false;
        }
    }

    public async Task<bool> DeletePostsOfUserAsync(string userId)
    {
        try
        {
            var foundPosts = await GetPostsOfUserAsync(userId);

            if (foundPosts != null && foundPosts.Any())
            {
                _context.Posts.RemoveRange(foundPosts);
                await _context.SaveChangesAsync();

                _logger.LogInformation(0403, "Successfully deleted posts of user with UserId:{UserId}.", userId);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(2407, ex, "An error occurred while trying to delete posts of user with UserId:{UserId}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", userId, ex.Message, ex.StackTrace);
            return false;
        }
    }

    public async Task<PostVote> GetPostVoteAsync(int id)
    {
        try
        {
            return await _context.PostVotes.FirstOrDefaultAsync(postVote => postVote.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(2408, ex, "An error occurred while trying to retrieve post vote with PostVoteId:{PostVoteId}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<int> CreatePostVoteAsync(PostVote postVote)
    {
        try
        {
            var result = await _context.PostVotes.AddAsync(postVote);
            await _context.SaveChangesAsync();

            _logger.LogInformation(0404, "Successfully created postVote." +
                "PostVoteId:{PostVoteId}, IsPositive:{IsPositive}, PostId:{PostId}, UserId:{UserId}",
                result.Entity.Id, postVote.IsPositive, postVote.PostId, postVote.UserId);

            return result.Entity.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(2409, ex, "An error occurred while trying to create postVote. " +
                "IsPositive:{IsPositive}, PostId:{PostId}, UserId:{UserId}" +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.",
                postVote.IsPositive, postVote.PostId, postVote.UserId, ex.Message, ex.StackTrace);
            return -1;
        }
    }

    public async Task<bool> UpdatePostVoteAsync(int postVoteId, PostVote postVote)
    {
        try
        {
            PostVote foundPostVote = await GetPostVoteAsync(postVoteId);
            if (foundPostVote is null)
            {
                _logger.LogWarning(1402, "Attempted to update null postVote, given postVoteId:{postVoteId}.", postVoteId);
                return false;
            }

            foundPostVote.IsPositive = postVote.IsPositive;

            await _context.SaveChangesAsync();

            _logger.LogInformation(0405, "Successfully updated postVote." +
                "PostVoteId:{PostVoteId}, IsPositive:{IsPositive}, PostId:{PostId}, UserId:{UserId}",
                postVoteId, postVote.IsPositive, postVote.PostId, postVote.UserId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(2410, ex, "An error occurred while trying to update postVote. " +
                "PostVoteId:{PostVoteId}, IsPositive:{IsPositive}, PostId:{PostId}, UserId:{UserId}" +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.",
                postVoteId, postVote.IsPositive, postVote.PostId, postVote.UserId, ex.Message, ex.StackTrace);
            return false;
        }
    }

    public async Task<bool> DeletePostVoteAsync(int id)
    {
        try
        {
            PostVote foundPostVote = await GetPostVoteAsync(id);
            if (foundPostVote is null)
            {
                _logger.LogWarning(1403, "Attempted to delete null postVote, given postVoteId:{postVoteId}.", id);
                return false;
            }

            _context.PostVotes.Remove(foundPostVote);
            await _context.SaveChangesAsync();

            _logger.LogInformation(0406, "Successfully deleted postVote with PostVoteId:{PostVoteId}.", id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(2411, ex, "An error occurred while trying to delete postVote with PostVoteId:{PostVoteId}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", id, ex.Message, ex.StackTrace);
            return false;
        }
    }
}

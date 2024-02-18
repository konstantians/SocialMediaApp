using Microsoft.EntityFrameworkCore;
using SocialMediaApp.SharedModels;

namespace SocialMediaApp.DataAccessLibrary.Repositories;

public class PostDataAccess : IPostDataAccess
{
    private readonly AppDbContext _context;

    public PostDataAccess(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Post>> GetPostsAsync()
    {
        try
        {
            return await _context.Posts.ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public async Task<IEnumerable<Post>> GetPostsOfUserAsync(string userId)
    {
        try
        {
            return await _context.Posts.Where(post => post.UserId == userId).ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public async Task<Post> GetPostAsync(int id)
    {
        try
        {
            return await _context.Posts.FirstOrDefaultAsync(post => post.Id == id);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public async Task<int> CreatePostAsync(Post post)
    {
        try
        {
            var result = await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();

            return result.Entity.Id;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return -1;
        }
    }

    public async Task<bool> UpdatePostAsync(int postId, Post post)
    {
        try
        {
            Post foundPost = await GetPostAsync(postId);
            if (foundPost is null)
                return false;

            foundPost.SentAt = post.SentAt;
            foundPost.Title = post.Title;
            foundPost.Content = post.Content;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    public async Task<bool> DeletePostAsync(int id)
    {
        try
        {
            Post foundPost = await GetPostAsync(id);
            if (foundPost is null)
                return false;

            _context.Posts.Remove(foundPost);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
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
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
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
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public async Task<int> CreatePostVote(PostVote postVote)
    {
        try
        {
            var result = await _context.PostVotes.AddAsync(postVote);
            await _context.SaveChangesAsync();

            return result.Entity.Id;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return -1;
        }
    }

    public async Task<bool> UpdatePostVote(int postVoteId, PostVote postVote)
    {
        try
        {
            PostVote foundPostVote = await GetPostVoteAsync(postVoteId);
            if (foundPostVote is null)
                return false;

            foundPostVote.IsPositive = postVote.IsPositive;

            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    public async Task<bool> DeletePostVote(int id)
    {
        try
        {
            PostVote foundPostVote = await GetPostVoteAsync(id);
            if (foundPostVote is null)
                return false;

            _context.PostVotes.Remove(foundPostVote);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }
}

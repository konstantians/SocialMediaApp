using SocialMediaApp.SharedModels;

namespace SocialMediaApp.DataAccessLibrary.Repositories
{
    public interface IPostDataAccess
    {
        Task<int> CreatePostAsync(Post post);
        Task<int> CreatePostVoteAsync(PostVote postVote);
        Task<bool> DeletePostAsync(int id);
        Task<bool> DeletePostsOfUserAsync(string userId);
        Task<bool> DeletePostVoteAsync(int id);
        Task<Post> GetPostAsync(int id);
        Task<IEnumerable<Post>> GetPostsAsync();
        Task<IEnumerable<Post>> GetPostsAsync(int amount);
        Task<IEnumerable<Post>> GetPostsOfUserAsync(string userId);
        Task<PostVote> GetPostVoteAsync(int id);
        Task<bool> UpdatePostAsync(int postId, Post post);
        Task<bool> UpdatePostVoteAsync(int postVoteId, PostVote postVote);
    }
}
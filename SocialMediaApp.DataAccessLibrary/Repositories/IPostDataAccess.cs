using SocialMediaApp.SharedModels;

namespace SocialMediaApp.DataAccessLibrary.Repositories
{
    public interface IPostDataAccess
    {
        Task<int> CreatePostAsync(Post post);
        Task<int> CreatePostVote(PostVote postVote);
        Task<bool> DeletePostAsync(int id);
        Task<bool> DeletePostsOfUserAsync(string userId);
        Task<bool> DeletePostVote(int id);
        Task<Post> GetPostAsync(int id);
        Task<IEnumerable<Post>> GetPostsAsync();
        Task<IEnumerable<Post>> GetPostsOfUserAsync(string userId);
        Task<PostVote> GetPostVoteAsync(int id);
        Task<bool> UpdatePostAsync(int postId, Post post);
        Task<bool> UpdatePostVote(int postVoteId, PostVote postVote);
    }
}
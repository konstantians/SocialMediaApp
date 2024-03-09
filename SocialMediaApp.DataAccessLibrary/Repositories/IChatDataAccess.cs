using SocialMediaApp.SharedModels;

namespace SocialMediaApp.DataAccessLibrary.Repositories
{
    public interface IChatDataAccess
    {
        Task<bool> AddUserToChatAsync(string userId, int chatId);
        Task<int> CreateChatAsync(Chat chat);
        Task<bool> DeleteChatAsync(int id);
        Task<Chat> GetChatAsync(int id);
        Task<IEnumerable<Chat>> GetChatsAsync();
        Task<IEnumerable<Chat>> GetChatsOfUserAsync(string userId);
        Task<bool> LeaveChatAsync(string userId, int chatId);
    }
}
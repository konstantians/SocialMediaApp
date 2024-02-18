using SocialMediaApp.SharedModels;

namespace SocialMediaApp.DataAccessLibrary.Repositories
{
    public interface IMessageDataAccess
    {
        Task<int> CreateMessageAsync(Message message);
        Task<bool> DeleteMessageAsync(int id);
        Task<Message> GetMessageAsync(int id);
        Task<IEnumerable<Message>> GetMessagesAsync();
        Task<bool> UpdateMessageAsync(int messageId, Message message);
    }
}
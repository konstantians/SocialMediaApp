using SocialMediaApp.SharedModels;

namespace SocialMediaApp.DataAccessLibrary.Repositories
{
    public interface IMessageDataAccess
    {
        Task<int> CreateMessageAsync(Message message);
        Task<int> CreateMessageStatusAsync(MessageStatus messageStatus);
        Task<bool> DeleteMessageAsync(int id);
        Task<Message> GetMessageAsync(int id);
        Task<IEnumerable<Message>> GetMessagesAsync();
        Task<MessageStatus> GetMessageStatusAsync(int id);
        Task<bool> UpdateMessageAsync(int messageId, Message message);
        Task<bool> UpdateMessageStatusAsync(int messageStatusId, MessageStatus messageStatus);
    }
}
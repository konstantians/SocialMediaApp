using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SocialMediaApp.SharedModels;

namespace SocialMediaApp.DataAccessLibrary.Repositories;

public class MessageDataAccess : IMessageDataAccess
{
    private readonly AppDbContext _context;
    private readonly ILogger<MessageDataAccess> _logger;

    public MessageDataAccess(AppDbContext context, ILogger<MessageDataAccess> logger = null!)
    {
        _context = context;
        _logger = logger ?? NullLogger<MessageDataAccess>.Instance;
    }

    public async Task<IEnumerable<Message>> GetMessagesAsync()
    {
        try
        {
            return await _context.Messages.Include(message => message.Notifications).
                Include(message => message.MessageStatuses).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(2300, ex, "An error occurred while trying to retrieve application's messages. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<Message> GetMessageAsync(int id)
    {
        try
        {
            return await _context.Messages
                .Include(message => message.Notifications)
                .Include(message => message.MessageStatuses)
                .FirstOrDefaultAsync(message => message.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(2301, ex, "An error occurred while trying to retrieve message with MessageId:{MessageId}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<int> CreateMessageAsync(Message message)
    {
        try
        {
            var result = await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();

            _logger.LogInformation(0300, "Successfully created message." +
                "MessageId:{MessageId}, SentAt:{SentAt}, Content:{Content}, UserId:{UserId}, ChatId:{ChatId}",
                result.Entity.Id, message.SentAt, message.Content, message.UserId, message.ChatId);
            return result.Entity.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(2302, ex, "An error occurred while trying to create message. " +
                "SentAt:{SentAt}, Content:{Content}, UserId:{UserId}, ChatId:{ChatId}" +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.",
                message.SentAt, message.Content, message.UserId, message.ChatId, ex.Message, ex.StackTrace);
            return -1;
        }
    }

    public async Task<bool> UpdateMessageAsync(int messageId, Message message)
    {
        try
        {
            Message foundMessage = await GetMessageAsync(messageId);
            if (foundMessage is null)
            {
                _logger.LogWarning(1300, "Attempted to update null message, given messageId:{messageId}.", messageId);
                return false;
            }

            foundMessage.SentAt = message.SentAt;
            foundMessage.Content = message.Content;
            await _context.SaveChangesAsync();

            _logger.LogInformation(0301, "Successfully updated message." +
                "MessageId:{MessageId}, SentAt:{SentAt}, Content:{Content}, UserId:{UserId}, ChatId:{ChatId}",
                messageId, message.SentAt, message.Content, message.UserId, message.ChatId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(2303, ex, "An error occurred while trying to update message. " +
                "MessageId:{MessageId}, SentAt:{SentAt}, Content:{Content}, UserId:{UserId}, ChatId:{ChatId}" +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.",
                messageId, message.SentAt, message.Content, message.UserId, message.ChatId, ex.Message, ex.StackTrace);
            return false;
        }
    }

    public async Task<bool> DeleteMessageAsync(int id)
    {
        try
        {
            Message foundMessage = await GetMessageAsync(id);
            if (foundMessage is null)
            {
                _logger.LogWarning(1301, "Attempted to delete null message, given messageId:{messageId}.", id);
                return false;
            }

            _context.Messages.Remove(foundMessage);
            await _context.SaveChangesAsync();

            _logger.LogInformation(0302, "Successfully deleted message with MessageId:{MessageId}.", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(2304, ex, "An error occurred while trying to delete message with MessageId:{MessageId}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", id, ex.Message, ex.StackTrace);
            return false;
        }
    }

    public async Task<MessageStatus> GetMessageStatusAsync(int id)
    {
        try
        {
            return await _context.MessageStatuses.FirstOrDefaultAsync(message => message.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(2305, ex, "An error occurred while trying to retrieve messageStatus with " +
                "MessageStatusId:{MessageStatusId}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", id, ex.Message, ex.StackTrace);
            throw;
        }
    }


    public async Task<int> CreateMessageStatusAsync(MessageStatus messageStatus)
    {
        try
        {
            var result = await _context.MessageStatuses.AddAsync(messageStatus);
            await _context.SaveChangesAsync();

            _logger.LogInformation(0303, "Successfully created messageStatus. " +
                "MessageStatusId:{MessageStatusId}, IsSeen:{IsSeen}, UserId:{UserId}, MessageId:{MessageId}.",
                result.Entity.Id, messageStatus.IsSeen, messageStatus.UserId, messageStatus.MessageId);
            return result.Entity.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(2306, ex, "An error occurred while trying to create messageStatus." +
                "IsSeen:{IsSeen}, UserId:{UserId}, MessageId:{MessageId}. ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", 
                messageStatus.IsSeen, messageStatus.UserId, messageStatus.MessageId, ex.Message, ex.StackTrace);
            return -1;
        }
    }

    public async Task<bool> UpdateMessageStatusAsync(int messageStatusId, MessageStatus messageStatus)
    {
        try
        {
            MessageStatus foundMessageStatus = await GetMessageStatusAsync(messageStatusId);
            if (foundMessageStatus is null)
            {
                _logger.LogWarning(1302, "Attempted to update null messageStatus, given messageStatusId:{messageStatusId}.", 
                    messageStatusId);
                return false;
            }

            foundMessageStatus.IsSeen = messageStatus.IsSeen;
            await _context.SaveChangesAsync();

            _logger.LogInformation(0304, "Successfully updated messageStatus. " +
                "MessageStatusId:{MessageStatusId}, IsSeen:{IsSeen}, UserId:{UserId}, MessageId:{MessageId}.",
                messageStatusId, messageStatus.IsSeen, messageStatus.UserId, messageStatus.MessageId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(2307, ex, "An error occurred while trying to update messageStatus." +
                "MessageStatusId:{MessageStatusId}, IsSeen:{IsSeen}, UserId:{UserId}, MessageId:{MessageId}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.",
                messageStatus.Id, messageStatus.IsSeen, messageStatus.UserId, messageStatus.MessageId, ex.Message, ex.StackTrace);
            return false;
        }
    }
}

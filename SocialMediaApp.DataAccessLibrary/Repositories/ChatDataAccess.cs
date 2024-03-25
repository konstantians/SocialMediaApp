using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SocialMediaApp.SharedModels;
using System;

namespace SocialMediaApp.DataAccessLibrary.Repositories;

public class ChatDataAccess : IChatDataAccess
{
    private readonly AppDbContext _context;
    private readonly ILogger<ChatDataAccess> _logger;

    public ChatDataAccess(AppDbContext context, ILogger<ChatDataAccess> logger = null!)
    {
        _context = context;
        _logger = logger ?? NullLogger<ChatDataAccess>.Instance;
    }

    public async Task<IEnumerable<Chat>> GetChatsAsync()
    {
        try
        {
            return await _context.Chats
                .Include(chat => chat.ChatsUsers)
                .Include(chat => chat.Messages)
                .ThenInclude(message => message.MessageStatuses)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(2200, ex, "An error occurred while trying to retrieve application's chat. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<IEnumerable<Chat>> GetChatsOfUserAsync(string userId)
    {
        try
        {
            return await _context.Chats
                .Include(chat => chat.ChatsUsers)
                .Include(chat => chat.Messages)
                .ThenInclude(message => message.MessageStatuses)
                .Where(chat => chat.ChatsUsers.Any(cu => cu.UserId == userId)).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(2201, ex, "An error occurred while trying to retrieve user's chat. " +
                "UserId:{UserId}. ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", 
                userId, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<Chat> GetChatAsync(int id)
    {
        try
        {
            return await _context.Chats
                .Include(chat => chat.ChatsUsers)
                .Include(chat => chat.Messages)
                .ThenInclude(message => message.MessageStatuses)
                .FirstOrDefaultAsync(chat => chat.Id == id);
        }
        catch (Exception ex)
        {

            _logger.LogError(2202, ex, "An error occurred while trying to retrieve chat with ChatId:{ChatId}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<int> CreateChatAsync(Chat chat)
    {
        try
        {
            var result = await _context.Chats.AddAsync(chat);
            await _context.SaveChangesAsync();

            _logger.LogInformation(0200, "Successfully created chat with ChatId:{ChatId}", result.Entity.Id);
            return result.Entity.Id;
        }
        catch (Exception ex)
        {

            _logger.LogError(2203, ex, "An error occurred while trying to create chat {ChatId}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", chat.Id, ex.Message, ex.StackTrace);
            return -1;
        }
    }

    public async Task<bool> AddUserToChatAsync(string userId, int chatId)
    {
        try
        {
            Chat foundChat = await GetChatAsync(chatId);
            if (foundChat is null)
            {

                _logger.LogWarning(1200, "Attempted to add user with UserId:{UserId} from null chat, given ChatId:{ChatId}.",
                    userId, chatId);
                return false;
            }

            // Check if the user is already in the chat
            if (foundChat.ChatsUsers.Any(chatsUsers => chatsUsers.UserId == userId))
            {
                _logger.LogWarning(1201, "User with UserId:{UserId} already participates in chat with ChatId:{ChatId}.", userId, chatId);
                return false;
            }

            ChatsUsers chatsUsers = new() { ChatId = chatId, UserId = userId };
            foundChat.ChatsUsers.Add(chatsUsers);

            await _context.SaveChangesAsync();

            _logger.LogInformation(0201, "Successfully added user with UserId:{UserId} to Chat with ChatId:{ChatId}", userId, chatId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(2204, ex, "An error occurred while trying to add user to chat. " +
                "UserId:{UserId}, ChatId:{chatId}. ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.",
                userId, chatId, ex.Message, ex.StackTrace);
            return false;
        }
    }

    public async Task<bool> LeaveChatAsync(string userId, int chatId)
    {
        try
        {
            Chat foundChat = await GetChatAsync(chatId);
            if (foundChat is null)
            {
                _logger.LogWarning(1202, "Attempted to remove user with UserId:{UserId} from null chat, given ChatId:{ChatId}.", 
                    userId, chatId);
                return false;
            }

            ChatsUsers chatsUsers = foundChat.ChatsUsers.FirstOrDefault(chatUser => chatUser.UserId == userId);
            if (chatsUsers is null)
            {
                _logger.LogWarning(1203, "User with UserId:{UserId} is not member of chat with ChatId:{ChatId}.", userId, chatId);
                return false;
            }

            foundChat.ChatsUsers.Remove(chatsUsers);

            await _context.SaveChangesAsync();

            _logger.LogInformation(0202, "Successfully removed user with UserId:{UserId} from chat with ChatId:{ChatId}", userId, chatId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(2205, ex, "An error occurred while trying to remove user from chat. " +
                "UserId:{UserId}, ChatId:{chatId}. ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.",
                userId, chatId, ex.Message, ex.StackTrace);
            return false;
        }
    }

    public async Task<bool> DeleteChatAsync(int id)
    {
        try
        {
            Chat foundChat = await GetChatAsync(id);
            if (foundChat is null)
            {
                _logger.LogWarning(1204, "Attempted to delete null chat, given ChatId:{ChatId}.", id);
                return false;
            }

            _context.Chats.Remove(foundChat);
            await _context.SaveChangesAsync();

            _logger.LogInformation(0203, "Successfully deleted chat with ChatId:{ChatId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(2206, ex, "An error occurred while trying to delete chat {chatId}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", id, ex.Message, ex.StackTrace);
            return false;
        }
    }
}

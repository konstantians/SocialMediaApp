using Microsoft.EntityFrameworkCore;
using SocialMediaApp.SharedModels;

namespace SocialMediaApp.DataAccessLibrary.Repositories;

public class ChatDataAccess : IChatDataAccess
{
    private readonly AppDbContext _context;

    public ChatDataAccess(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Chat>> GetChatsAsync()
    {
        try
        {
            return await _context.Chats
                .Include(chat => chat.Messages)
                .Include(chat => chat.ChatsUsers)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public async Task<IEnumerable<ChatsUsers>> GetChatsOfUserAsync(string userId)
    {
        try
        {
            return await _context.ChatsUsers
                .Include(chatsUsers => chatsUsers.Chat)
                .Where(chatsUsers => chatsUsers.UserId == userId).ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public async Task<Chat> GetChatAsync(int id)
    {
        try
        {
            return await _context.Chats
                .Include(chat => chat.Messages)
                .Include(chat => chat.ChatsUsers)
                .FirstOrDefaultAsync(chat => chat.Id == id);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public async Task<int> CreateChatAsync(Chat chat)
    {
        try
        {
            var result = await _context.Chats.AddAsync(chat);
            await _context.SaveChangesAsync();

            return result.Entity.Id;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return -1;
        }
    }

    public async Task<bool> AddUserToChatAsync(string userId, int chatId)
    {
        try
        {
            Chat foundChat = await GetChatAsync(chatId);
            if (foundChat is null)
                return false;

            // Check if the user is already in the chat
            if (foundChat.ChatsUsers.Any(chatsUsers => chatsUsers.UserId == userId))
                return false;

            ChatsUsers chatsUsers = new() { ChatId = chatId, UserId = userId };
            foundChat.ChatsUsers.Add(chatsUsers);

            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    public async Task<bool> LeaveChatAsync(string userId, int chatId)
    {
        try
        {
            Chat foundChat = await GetChatAsync(chatId);
            if (foundChat is null)
                return false;

            ChatsUsers chatsUsers = foundChat.ChatsUsers.FirstOrDefault(chatUser => chatUser.UserId == userId);
            if (chatsUsers is null)
                return false;

            foundChat.ChatsUsers.Remove(chatsUsers);

            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    public async Task<bool> DeleteChatAsync(int id)
    {
        try
        {
            Chat foundChat = await GetChatAsync(id);
            if (foundChat is null)
                return false;

            _context.Chats.Remove(foundChat);
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

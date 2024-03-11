using Microsoft.EntityFrameworkCore;
using SocialMediaApp.SharedModels;

namespace SocialMediaApp.DataAccessLibrary.Repositories;

public class MessageDataAccess : IMessageDataAccess
{
    private readonly AppDbContext _context;
    public MessageDataAccess(AppDbContext context)
    {
        _context = context;
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
            Console.WriteLine(ex.Message);
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
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public async Task<int> CreateMessageAsync(Message message)
    {
        try
        {
            var result = await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();

            return result.Entity.Id;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return -1;
        }
    }

    public async Task<bool> UpdateMessageAsync(int messageId, Message message)
    {
        try
        {
            Message foundMessage = await GetMessageAsync(messageId);
            if (foundMessage is null)
                return false;

            foundMessage.SentAt = message.SentAt;
            foundMessage.Content = message.Content;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    public async Task<bool> DeleteMessageAsync(int id)
    {
        try
        {
            Message foundMessage = await GetMessageAsync(id);
            if (foundMessage is null)
                return false;

            _context.Messages.Remove(foundMessage);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
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
            Console.WriteLine(ex.Message);
            throw;
        }
    }


    public async Task<int> CreateMessageStatusAsync(MessageStatus messageStatus)
    {
        try
        {
            var result = await _context.MessageStatuses.AddAsync(messageStatus);
            await _context.SaveChangesAsync();

            return result.Entity.Id;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return -1;
        }
    }

    public async Task<bool> UpdateMessageStatusAsync(int messageStatusId, MessageStatus messageStatus)
    {
        try
        {
            MessageStatus foundMessageStatus = await GetMessageStatusAsync(messageStatusId);
            if (foundMessageStatus is null)
                return false;

            foundMessageStatus.IsSeen = messageStatus.IsSeen;
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

using Microsoft.EntityFrameworkCore;
using SocialMediaApp.SharedModels;

namespace SocialMediaApp.DataAccessLibrary.Repositories;

public class NotificationDataAccess : INotificationDataAccess
{
    private readonly AppDbContext _context;

    public NotificationDataAccess(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Notification>> GetNotificationAsync()
    {
        try
        {
            return await _context.Notifications
                .Include(notification => notification.Message)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public async Task<IEnumerable<Notification>> GetNotificationsOfUserAsync(string userId)
    {
        try
        {
            return await _context.Notifications
                .Include(notification => notification.Message)
                .Where(notification => notification.ToUserId == userId).ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }
    
    public async Task<Notification> GetNotificationAsync(int id)
    {
        try
        {
            return await _context.Notifications
                .Include(notification => notification.Message)
                .FirstOrDefaultAsync(notification => notification.Id == id);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public async Task<int> CreateNotificationAsync(Notification notification)
    {
        try
        {
            var result = await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();

            return result.Entity.Id;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return -1;
        }
    }

    public async Task<bool> DeleteNotificationAsync(int id)
    {
        try
        {
            Notification foundNotification = await GetNotificationAsync(id);
            if (foundNotification is null)
                return false;

            _context.Notifications.Remove(foundNotification);
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

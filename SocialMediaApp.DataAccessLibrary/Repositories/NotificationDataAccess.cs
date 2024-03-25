using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using SocialMediaApp.SharedModels;

namespace SocialMediaApp.DataAccessLibrary.Repositories;

public class NotificationDataAccess : INotificationDataAccess
{
    private readonly AppDbContext _context;
    private readonly ILogger<NotificationDataAccess> _logger;

    public NotificationDataAccess(AppDbContext context, ILogger<NotificationDataAccess> logger)
    {
        _context = context;
        _logger = logger ?? NullLogger<NotificationDataAccess>.Instance;
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
            _logger.LogError(2500, ex, "An error occurred while trying to retrieve application's notifications. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", ex.Message, ex.StackTrace);
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
            _logger.LogError(2501, ex, "An error occurred while trying to retrieve notifications of user with UserId:{UserId}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", userId, ex.Message, ex.StackTrace);
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
            _logger.LogError(2502, ex, "An error occurred while trying to retrieve notification with NotificationId:{NotificationId}. "+
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<int> CreateNotificationAsync(Notification notification)
    {
        try
        {
            var result = await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();

            _logger.LogInformation(0500, "Successfully created notification. " +
                "NotificationId:{NotificationId}, SentAt:{SentAt}, FromUserId:{FromUserId}, ToUserId:{ToUserId}. ",
                 result.Entity.Id, notification.SentAt, notification.FromUserId, notification.ToUserId);

            return result.Entity.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(2503, ex, "An error occurred while trying to create notification. " +
                "SentAt:{SentAt}, FromUserId:{FromUserId}, ToUserId:{ToUserId}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.",
                notification.SentAt, notification.FromUserId, notification.ToUserId, ex.Message, ex.StackTrace);
            return -1;
        }
    }

    public async Task<bool> DeleteNotificationAsync(int id)
    {
        try
        {
            Notification foundNotification = await GetNotificationAsync(id);
            if (foundNotification is null)
            {
                _logger.LogWarning(1500, "Attempted to delete null notification, given NotificationId:{NotificationId}", id);
                return false;
            }

            _context.Notifications.Remove(foundNotification);
            await _context.SaveChangesAsync();

            _logger.LogInformation(0501, "Successfully deleted notification with NotificationId:{NotificationId}. ", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(2504, ex, "An error occurred while trying to delete notification with NotificationId:{NotificationId}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", id, ex.Message, ex.StackTrace);
            return false;
        }
    }
}

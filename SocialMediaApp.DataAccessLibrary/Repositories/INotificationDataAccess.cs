using SocialMediaApp.SharedModels;

namespace SocialMediaApp.DataAccessLibrary.Repositories
{
    public interface INotificationDataAccess
    {
        Task<int> CreateNotificationAsync(Notification notification);
        Task<bool> DeleteNotificationAsync(int id);
        Task<IEnumerable<Notification>> GetNotificationAsync();
        Task<Notification> GetNotificationAsync(int id);
        Task<IEnumerable<Notification>> GetNotificationsOfUserAsync(string userId);
    }
}
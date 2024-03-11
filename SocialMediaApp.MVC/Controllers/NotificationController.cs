using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.AuthenticationLibrary;
using SocialMediaApp.DataAccessLibrary.Repositories;
using SocialMediaApp.SharedModels;

namespace SocialMediaApp.MVC.Controllers;

public class NotificationController : Controller
{
    private readonly INotificationDataAccess _notificationDataAccess;
    private readonly IAuthenticationProcedures _authenticationProcedures;

    public NotificationController(INotificationDataAccess notificationDataAccess, IAuthenticationProcedures authenticationProcedures)
    {
        _notificationDataAccess = notificationDataAccess;
        _authenticationProcedures = authenticationProcedures;
    }

    [Authorize]
    public async Task<IActionResult> ViewNotifications(bool failedNotificationDeletion, bool successfulNotificationDeletion)
    {
        AppUser appUser = await _authenticationProcedures.GetCurrentUserAsync();

        var result = await _notificationDataAccess.GetNotificationsOfUserAsync(appUser.Id);
        List<Notification> notifications = result.OrderByDescending(notification => notification.SentAt).ToList();
        foreach (Notification notification in notifications)
        {
            notification.Sender = await _authenticationProcedures.FindByUserIdAsync(notification.FromUserId);
        }

        ViewData["FailedNotificationDeletion"] = failedNotificationDeletion;
        ViewData["SuccessfulNotificationDeletion"] = successfulNotificationDeletion;
        return View(notifications);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> DeleteNotification(int notificationId)
    {
        AppUser appUser = await _authenticationProcedures.GetCurrentUserAsync();
        var userNotifications = await _notificationDataAccess.GetNotificationsOfUserAsync(appUser.Id);
        bool userOwnsPost = userNotifications.ToList().Any(notification => notification.Id == notificationId);
        if (!ModelState.IsValid || !userOwnsPost)
        {
            return RedirectToAction("ViewNotifications", "Notification");
        }

        var result = await _notificationDataAccess.DeleteNotificationAsync(notificationId);
        if (!result)
            return RedirectToAction("ViewNotifications", "Notification", new { failedNotificationDeletion = true });

        return RedirectToAction("ViewNotifications", "Notification", new { successfulNotificationDeletion = true });
    }
}

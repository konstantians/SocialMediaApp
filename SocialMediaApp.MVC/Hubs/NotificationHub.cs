using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SocialMediaApp.AuthenticationLibrary;
using SocialMediaApp.DataAccessLibrary.Repositories;
using SocialMediaApp.SharedModels;
using System.Collections.Generic;

namespace SocialMediaApp.MVC.Hubs;

public class NotificationHub : Hub
{
    private readonly IAuthenticationProcedures _authenticationProcedures;
    private readonly INotificationDataAccess _notificationDataAccess;

    public NotificationHub(IAuthenticationProcedures authenticationProcedures, INotificationDataAccess notificationDataAccess)
    {
        _authenticationProcedures = authenticationProcedures;
        _notificationDataAccess = notificationDataAccess;
    }

    public async Task StoreSignalRConnectionId()
    {
        AppUser appUser = await _authenticationProcedures.GetCurrentUserAsync();
        await _authenticationProcedures.UpdateSignalRConnectionIdOfUser(appUser.Id, Context.ConnectionId);
    }

    public async Task<string> SendFriendNotification(string username, string email)
    {
        AppUser appUser = await _authenticationProcedures.GetCurrentUserAsync();
        AppUser friend;
        if (username != "" || username is not null)
            friend = await _authenticationProcedures.FindByUsernameAsync(username);
        else
            friend = await _authenticationProcedures.FindByEmailAsync(email);

        if (friend is null)
            return ("That user is already in your friends list.|danger");

        if (friend.Id == appUser.Id)
            return ("You can not send a friend request to yourself.|danger");

        if (appUser.Friendships.Any(friendship => friendship.UserId == friend.Id || friendship.FriendId == friend.Id))
            return ("That user is already in your friends list.|danger");

        Notification notification = new Notification();
        notification.SentAt = DateTime.Now;
        notification.FromUserId = appUser.Id;
        notification.ToUserId = friend.Id;
        notification.NewFriendRequest = true;
        notification.MessageId = null;

        int result = await _notificationDataAccess.CreateNotificationAsync(notification);

        if (result == -1)
            return ("Unfortunately a friend notification could not be sent to the user.\n" +
                   "Please try again or contact us through our email kinnaskonstantinos0@gmail.com.|danger");

        if (friend.SignalRConnectionId is not null)
        {
            var notificationsOfUser = await _notificationDataAccess.GetNotificationsOfUserAsync(friend.Id);
            await Clients.Client(friend.SignalRConnectionId!).SendAsync("UpdateNotificationCount", notificationsOfUser.Count());
        }

        return ("A friend notification has successfully been sent to the chosen user account!|success");
    }

    [Authorize]
    public async Task<string> RejectFriendInvitation(int notificationId, string senderId)
    {
        AppUser appUser = await _authenticationProcedures.GetCurrentUserAsync();
        AppUser friend = await _authenticationProcedures.FindByUserIdAsync(senderId);

        if(friend is null)
            return ("The user does not seem to exist.\n" +
                   "Please try to contact them to see if something is wrong on their end.|danger|0");

        await _notificationDataAccess.DeleteNotificationAsync(notificationId);
        var notificationsOfUser = await _notificationDataAccess.GetNotificationsOfUserAsync(appUser.Id);
        int notificationCount = notificationsOfUser.Count();
        await Clients.Client(appUser.SignalRConnectionId!).SendAsync("UpdateNotificationCount", notificationCount);

        Notification notification = new Notification();
        notification.SentAt = DateTime.Now;
        notification.FromUserId = appUser.Id;
        notification.ToUserId = friend.Id;
        notification.FriendRequestRejected = true;
        notification.MessageId = null;

        int notificationCreated = await _notificationDataAccess.CreateNotificationAsync(notification);

        if (notificationCreated == -1)
            return ("The friend invitation has been successfully rejected, but a notification could not be sent to them.\n" +
                   "If you need to notify them of your decision send them a message through the app or contact them manually.|danger|0");

        if (friend.SignalRConnectionId is not null)
        {

            var notificationsOfFriend = await _notificationDataAccess.GetNotificationsOfUserAsync(friend.Id);
            await Clients.Client(friend.SignalRConnectionId!).SendAsync("UpdateNotificationCount", notificationsOfFriend.Count());
        }

        return ("The friend request has been successfully rejected and a notification has been sent " +
            $"to the user to notify them of your decision!|success|{notificationCount}");
    }

    [Authorize]
    public async Task<string> AcceptFriendInvitation(int notificationId, string senderId)
    {
        AppUser appUser = await _authenticationProcedures.GetCurrentUserAsync();
        AppUser friend = await _authenticationProcedures.FindByUserIdAsync(senderId);

        if (friend is null)
            return ("The user does not seem to exist.\n" +
                   "Please try to contact them to see if something is wrong on their end.|danger|0");

        bool result = await _authenticationProcedures.AddFriend(appUser.Id, senderId);
        if (!result)
            return ("Unfortunately the friend could not be added to your friend list." +
                   "Please try again or contact us through our email kinnaskonstantinos0@gmail.com.|danger|0");

        //send to the current user update on the notifications (-1)
        await _notificationDataAccess.DeleteNotificationAsync(notificationId);
        var notificationsOfUser = await _notificationDataAccess.GetNotificationsOfUserAsync(appUser.Id);
        int notificationCount = notificationsOfUser.Count();
        await Clients.Client(appUser.SignalRConnectionId!).SendAsync("UpdateNotificationCount", notificationCount);


        Notification notification = new Notification();
        notification.SentAt = DateTime.Now;
        notification.FromUserId = appUser.Id;
        notification.ToUserId = friend.Id;
        notification.FriendRequestAccepted = true;
        notification.MessageId = null;

        int notificationCreated = await _notificationDataAccess.CreateNotificationAsync(notification);

        if (notificationCreated == -1)
            return ("The user was added to your friend list, but we could not send to them a notification.\n" +
                   "If you need to notify them of your decision send them a message through the app or contact them manually.|danger|");

        
        if (friend.SignalRConnectionId is not null)
        {
            //sent to the friend update on the notifications (+1)
            var notificationsOfFriend = await _notificationDataAccess.GetNotificationsOfUserAsync(friend.Id);
            await Clients.Client(friend.SignalRConnectionId!).SendAsync("UpdateNotificationCount", notificationsOfFriend.Count());
        }

        return ("The friend request has been successfully accepted and a notification has been sent to " +
            $"the user to notify them of your decision!|success|{notificationCount}");
    }

}

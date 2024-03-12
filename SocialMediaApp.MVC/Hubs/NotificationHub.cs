using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SocialMediaApp.AuthenticationLibrary;
using SocialMediaApp.DataAccessLibrary.Repositories;
using SocialMediaApp.SharedModels;
using System.Text;

namespace SocialMediaApp.MVC.Hubs;

public class NotificationHub : Hub
{
    private readonly IAuthenticationProcedures _authenticationProcedures;
    private readonly INotificationDataAccess _notificationDataAccess;
    private readonly IChatDataAccess _chatDataAccess;
    private readonly IMessageDataAccess _messageDataAccess;

    public NotificationHub(IAuthenticationProcedures authenticationProcedures, INotificationDataAccess notificationDataAccess, 
            IChatDataAccess chatDataAccess, IMessageDataAccess messageDataAccess)
    {
        _authenticationProcedures = authenticationProcedures;
        _notificationDataAccess = notificationDataAccess;
        _chatDataAccess = chatDataAccess;
        _messageDataAccess = messageDataAccess;
    }

    [Authorize]
    public async Task StoreSignalRConnectionId()
    {
        AppUser appUser = await _authenticationProcedures.GetCurrentUserAsync();
        await _authenticationProcedures.UpdateSignalRConnectionIdOfUser(appUser.Id, Context.ConnectionId);
    }

    [Authorize]
    public async Task<string> SendFriendNotification(string username, string email)
    {
        if ((username is null || username == "") && (email is null || email == ""))
            return ("You need to fill the modal before submitting it.|danger");

        AppUser appUser = await _authenticationProcedures.GetCurrentUserAsync();
        AppUser friend;
        if (username != "" || username is not null)
            friend = await _authenticationProcedures.FindByUsernameAsync(username!);
        else
            friend = await _authenticationProcedures.FindByEmailAsync(email);

        if (friend is null && username != "")
            return ("There is not a registered user with the given username.|danger");
        else if (friend is null && email != "")
            return ("There is not a registered user with the given email.|danger");

        if (friend!.Id == appUser.Id)
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

    [Authorize]
    public async Task EnterChat(int chatId)
    {

        AppUser appUser = await _authenticationProcedures.GetCurrentUserAsync();
        await _authenticationProcedures.UpdateSignalRConnectionIdOfUser(appUser.Id, Context.ConnectionId);
        await _authenticationProcedures.UpdateUserChatStatus(appUser.Id, chatId);
        
        Chat chat = await _chatDataAccess.GetChatAsync(chatId);
        if (chat is null)
            return;

        StringBuilder updateMessagesStringBuilder = new StringBuilder();
        foreach (Message message in chat.Messages)
        {
            //if these are the user messages skip
            if (message.UserId == appUser.Id)
                continue;

            MessageStatus messageStatus = message.MessageStatuses.
                Where(messageStatus => messageStatus.UserId == appUser.Id && !messageStatus.IsSeen).FirstOrDefault()!;
            if (messageStatus is not null)
            {
                messageStatus.IsSeen = true;
                await _messageDataAccess.UpdateMessageStatusAsync(messageStatus.Id, messageStatus);
                updateMessagesStringBuilder.Append(message.Id);
                updateMessagesStringBuilder.Append("|");
            }
        }

        if (updateMessagesStringBuilder.Length == 0)
            return;

        updateMessagesStringBuilder.Remove(updateMessagesStringBuilder.Length - 1, 1);

        foreach (ChatsUsers chatUser in chat.ChatsUsers)
        {

            AppUser memberOfChat = await _authenticationProcedures.FindByUserIdAsync(chatUser.UserId!);
            if (memberOfChat.InChatWithId == chatId)
            {
                await Clients.Client(memberOfChat.SignalRConnectionId!).
                    SendAsync("UpdateSeenStatuses", appUser.UserName, updateMessagesStringBuilder.ToString());
            }
        }
    }

    [Authorize]
    public async Task SendMessage(string chatUsersIdsString, string message, string myColor)
    {
        AppUser appUser = await _authenticationProcedures.GetCurrentUserAsync();
        string[] userIds = chatUsersIdsString.Split('|');

        List<AppUser> membersOfChat = new List<AppUser>();
        StringBuilder seenStatusStringBuilder = new StringBuilder().Append("Seen By: ");
        foreach (string userId in userIds)
        {            
            //add the rest
            AppUser memberOfChat = await _authenticationProcedures.FindByUserIdAsync(userId);
            if(memberOfChat.InChatWithId == appUser.InChatWithId && memberOfChat.Id != appUser.Id)
            {
                seenStatusStringBuilder.Append(memberOfChat.UserName);
                seenStatusStringBuilder.Append(", ");
            }
            membersOfChat.Add(memberOfChat);
        }
        seenStatusStringBuilder.Remove(seenStatusStringBuilder.Length - 2, 2);

        string seenStatusString = seenStatusStringBuilder.ToString() == "Seen By" ? "Not Seen" : seenStatusStringBuilder.ToString();

        //create message for chat
        Message chatMessage = new Message();
        chatMessage.ChatId = (int)appUser.InChatWithId!;
        chatMessage.UserId = appUser.Id;
        chatMessage.Content = message;
        chatMessage.SentAt = DateTime.Now;
        int messageId = await _messageDataAccess.CreateMessageAsync(chatMessage);

        foreach (AppUser memberOfChat in membersOfChat)
        {
            if (appUser.Id != memberOfChat.Id)
            {
                MessageStatus messageStatus = new MessageStatus();
                messageStatus.UserId = memberOfChat.Id;
                messageStatus.MessageId = messageId;
                messageStatus.IsSeen = memberOfChat.InChatWithId == appUser.InChatWithId ? true : false;
                await _messageDataAccess.CreateMessageStatusAsync(messageStatus);
            }

            //if user in chat
            if (memberOfChat.InChatWithId == appUser.InChatWithId)
            {
                //send an asynchronous message to them
                await Clients.Client(memberOfChat.SignalRConnectionId!).
                    SendAsync("ReceiveMessage", appUser.UserName, @DateTime.Now.ToString("dd/MM/yyyy HH:mm"), 
                    message, seenStatusString, messageId.ToString(), myColor);
                continue;
            }

            //otherwise send notification to them
            Notification notification = new Notification();
            notification.MessageId = messageId;
            notification.FromUserId = appUser.Id;
            notification.ToUserId = memberOfChat.Id;
            notification.SentAt = DateTime.Now;
            await _notificationDataAccess.CreateNotificationAsync(notification);

            //update the count
            var notificationsOfUser = await _notificationDataAccess.GetNotificationsOfUserAsync(notification.ToUserId);
            await Clients.Client(memberOfChat.SignalRConnectionId!).SendAsync("UpdateNotificationCount", notificationsOfUser.Count());
        }
    }

    [Authorize]
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        AppUser appUser = await _authenticationProcedures.GetCurrentUserAsync();
        if(appUser is not null)
        {
            await _authenticationProcedures.UpdateUserChatStatus(appUser.Id, null);
        }

        await base.OnDisconnectedAsync(exception);
    }

}

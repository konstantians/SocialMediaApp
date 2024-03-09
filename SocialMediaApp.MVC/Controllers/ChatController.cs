using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.AuthenticationLibrary;
using SocialMediaApp.DataAccessLibrary.Repositories;
using SocialMediaApp.SharedModels;

namespace SocialMediaApp.MVC.Controllers;

public class ChatController : Controller
{
    private readonly IAuthenticationProcedures _authenticationProcedures;
    private readonly IChatDataAccess _chatDataAccess;
    private readonly INotificationDataAccess _notificationDataAccess;

    public ChatController(IAuthenticationProcedures authenticationProcedures,
        IChatDataAccess chatDataAccess, INotificationDataAccess notificationDataAccess)
    {
        _authenticationProcedures = authenticationProcedures;
        _chatDataAccess = chatDataAccess;
        _notificationDataAccess = notificationDataAccess;
    }

    [Authorize]
    public async Task<IActionResult> ViewChats(bool chatCreatedSuccessfully, bool leftChatSuccess, bool leftChatFailure)
    {
        AppUser appUser = await _authenticationProcedures.GetCurrentUserAsync();
        var result = await _chatDataAccess.GetChatsOfUserAsync(appUser.Id);
        List<Chat> chats = result.ToList();
        foreach (Chat chat in chats)
        {
            if(chat.Messages.Count > 0)
            {
                chat.Messages = chat.Messages.OrderByDescending(message => message.SentAt).ToList();
            }
            foreach (ChatsUsers chatUser in chat.ChatsUsers)
            {
                chatUser.AppUser = await _authenticationProcedures.FindByUserIdAsync(chatUser.UserId!);
            }
        }

        chats = chats.OrderByDescending(chat => chat.Messages.Count > 0 ? chat.Messages.FirstOrDefault().SentAt : DateTime.MinValue).ToList();


        List<string> appUsersUsernames = new List<string>();
        foreach (Friendship friendship in appUser.Friendships)
        {
            AppUser friend = friendship.UserId != appUser.Id ? await _authenticationProcedures.FindByUserIdAsync(friendship.UserId)
                : await _authenticationProcedures.FindByUserIdAsync(friendship.FriendId);
            appUsersUsernames.Add(friend.UserName!);
        }

        ViewData["FriendsUsernames"] = appUsersUsernames;
        ViewData["ChatCreatedSuccessfully"] = chatCreatedSuccessfully;
        ViewData["LeftChatSuccess"] = leftChatSuccess;
        ViewData["LeftChatFailure"] = leftChatFailure;
        return View(chats);
    }

    [Authorize]
    public async Task<IActionResult> ViewChat(int chatId)
    {
        Chat chat = await _chatDataAccess.GetChatAsync(chatId);
        return View(chat);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateChat(string[] friendsUsernames)
    {
        AppUser appUser = await _authenticationProcedures.GetCurrentUserAsync();

        int chatId = await _chatDataAccess.CreateChatAsync(new Chat());

        await _chatDataAccess.AddUserToChatAsync(appUser.Id, chatId);
        
        foreach (string friendUsername in friendsUsernames)
        {
            AppUser friend = await _authenticationProcedures.FindByUsernameAsync(friendUsername);
            await _chatDataAccess.AddUserToChatAsync(friend.Id, chatId);
        }

        return RedirectToAction("ViewChats", "Chat", new {chatCreatedSuccessfully = true});
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> LeaveChat(int chatId)
    {
        AppUser appUser = await _authenticationProcedures.GetCurrentUserAsync();
        Chat chat = await _chatDataAccess.GetChatAsync(chatId);

        bool result;
        if (chat.ChatsUsers.Count > 1) {
            result = await _chatDataAccess.LeaveChatAsync(appUser.Id, chatId);
        }
        else
        {
            result = await _chatDataAccess.DeleteChatAsync(chatId);
        }

        if (!result)
            return RedirectToAction("ViewChats", "Chat", new { leftChatFailure = true });


        return RedirectToAction("ViewChats", "Chat", new { leftChatSuccess = true });
    }
}

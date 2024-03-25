using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SocialMediaApp.Authentication;
using SocialMediaApp.SharedModels;
using System.Security.Claims;

namespace SocialMediaApp.AuthenticationLibrary;

public class AuthenticationProcedures : IAuthenticationProcedures
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly AppIdentityDbContext _identityContext;
    private readonly ILogger _logger;

    public AuthenticationProcedures(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, 
        AppIdentityDbContext context, ILogger<AuthenticationProcedures> logger = null!)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _identityContext = context;
        _logger = logger ?? NullLogger<AuthenticationProcedures>.Instance;
    }

    public async Task<List<AppUser>> GetUsersAsync()
    {
        try
        {
            //return await _userManager.Users.Include(user => user.Friendships).ToListAsync();
            return await _userManager.Users.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(2000, ex, "An error occurred while retrieving users. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<AppUser> GetCurrentUserAsync()
    {
        try
        {
            AppUser currentUser = await _userManager.GetUserAsync(_signInManager.Context.User);
            if (currentUser == null)
                return null!;
            return AddFriendshipsToUser(currentUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(2001, ex, "An error occurred while retrieving logged in user. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<AppUser> FindByUserIdAsync(string userId)
    {
        try
        {
            AppUser appUser = await _userManager.FindByIdAsync(userId);
            return AddFriendshipsToUser(appUser);
        }
        catch (Exception ex)
        {

            _logger.LogError(2002, ex, "An error occurred while retrieving user with id: {UserId}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", userId, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<AppUser> FindByUsernameAsync(string username)
    {
        try
        {
            AppUser appUser = await _userManager.FindByNameAsync(username);
            return AddFriendshipsToUser(appUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(2003, ex, "An error occurred while retrieving user with username: {Username}. " +
                "ExceptionMessage {ExceptionMessage}. StackTrace: {StackTrace}.", username, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<AppUser> FindByEmailAsync(string email)
    {
        try
        {
            AppUser appUser = await _userManager.FindByEmailAsync(email);
            return AddFriendshipsToUser(appUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(2004, ex, "An error occurred while retrieving user with email: {Email}. " +
                "ExceptionMessage {ExceptionMessage}. StackTrace: {StackTrace}.", email, ex.Message, ex.StackTrace);
            throw;
        }
    }

    private AppUser AddFriendshipsToUser(AppUser appUser)
    {
        try
        {
            if (appUser == null)
            {    
                _logger.LogWarning(1000,"Attempted to retrieve friendship of null user.");
                return null!;
            }

            appUser.Friendships = _identityContext.Friendships
            .Where(friend => friend.UserId == appUser.Id || friend.FriendId == appUser.Id)
            .ToList();
            return appUser;
        }
        catch (Exception ex)
        {
            _logger.LogError(2005, ex, "An error occurred while trying to retrieve user's friends. " +
                "UserId: {UserId}, Email: {Email}, Username: {Username}. " +
                "ExceptionMessage {ExceptionMessage}. StackTrace: {StackTrace}."
                , appUser.Id, appUser.Email, appUser.UserName, ex.Message, ex.StackTrace);
            throw;
        }
        
    }

    public async Task<(string, string)> RegisterUserAsync(AppUser appUser, string password,
        bool isPersistent)
    {
        try
        {
            var result = await _userManager.CreateAsync(appUser, password);
            if (!result.Succeeded)            
                throw new ApplicationException("Failed to create user account with given credentials.");

            string confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);

            _logger.LogInformation(0001,"Successfully created user account: UserId={UserId}, Email={Email}, Username={Username}.",
                appUser.Id, appUser.Email, appUser.UserName);

            return (appUser.Id, confirmationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(2006, ex, "An error occurred while creating user account. " +
                "Email: {Email}, Username: {Username}. ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}."
                , appUser.Email, appUser.UserName, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<bool> ConfirmEmailAsync(string userId, string confirmationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                _logger.LogWarning(1001, "Tried to confirm email of null user: " +
                    "UserId={UserId}, ConfirmationToken={ConfirmationToken}.", userId, confirmationToken);
                return false;
            }

            var result = await _userManager.ConfirmEmailAsync(user, confirmationToken);
            if (!result.Succeeded)
            {
                _logger.LogWarning(1002, "Email of user could not be confirmed: " +
                    "UserId={UserId}, ConfirmationToken={ConfirmationToken}. Errors={Errors}.", userId, confirmationToken, result.Errors);
                return false;
            }

            await _signInManager.SignInAsync(user, false);

            _logger.LogInformation(0001, "Successfully confirmed user's email account: UserId={UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(2007, ex, "An error occurred while confirming user email account. " +
                "UserId: {UserId}, ConfirmationToken: {ConfirmationToken}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}."
                , userId, confirmationToken, ex.Message, ex.StackTrace);
            throw;
        }
    }


    public async Task<(bool, string)> ChangePasswordAsync(AppUser appUser, string currentPassword, string newPassword)
    {
        try
        {
            IdentityResult result;
            if (currentPassword is not null)
            {
                result = await _userManager.ChangePasswordAsync(appUser, currentPassword, newPassword);
            }
            //this can happen if the user created an account through an external identity provider(edge case)
            else
            {
                result = await _userManager.AddPasswordAsync(appUser, newPassword);
            }

            if (!result.Succeeded && result.Errors.Where(error => error.Code == "PasswordMismatch").Count() > 0)
                return (false, "passwordMismatch");
            else if (!result.Succeeded)
            {
                _logger.LogWarning(1003, "Password could not be changed: " +
                    "UserId={UserId}, Email={Email}, Username={Username}. Errors={Errors}.",
                    appUser.Id, appUser.Email, appUser.UserName, result.Errors);
                return (false, "unknown");
            }

            _logger.LogInformation(0002, "Successfully changed user's account password. " +
                "UserId={UserId}, Email={Email}, Username={Username}.", appUser.Id, appUser.Email, appUser.UserName);
            return (true, "nothing");
        }
        catch (Exception ex)
        {
            _logger.LogError(2008, ex, "An error occurred while changing user account password. " +
                "UserId: {UserId}, Email: {Email}, Username: {Username}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}."
                , appUser.Id, appUser.Email, appUser.UserName, ex.Message, ex.StackTrace);
            return (false, "unknown");
        }
    }

    public async Task<bool> SignInUserAsync(string username, string password, bool isPersistent)
    {
        try
        {
            var result = await _signInManager.PasswordSignInAsync(username, password, isPersistent, false);
            if (!result.Succeeded)
                _logger.LogWarning(1004, "User could not be signed in. Username={Username}.", username);
            else
                _logger.LogInformation(0003, "Successfully signed in user. Username={Username}, IsPersistent={IsPersistent}.",
                username, isPersistent);
            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(2009, ex, "An error occurred while trying to sign in the user. " +
                "Username: {Username}. ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}."
                , username, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<bool> CheckIfUserIsLoggedIn()
    {
        try
        {
            return await Task.Run(() => _signInManager.IsSignedIn(_signInManager.Context.User));
        }
        catch (Exception ex)
        {
            _logger.LogError(2010, ex, "An error occurred while trying to check if the user was logged in. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<bool> UpdateUserAccountAsync(AppUser appUser)
    {
        try
        {
            var result = await _userManager.UpdateAsync(appUser);
            if (!result.Succeeded)
                _logger.LogWarning(1005, "User account information could not be updated. " +
                    "UserId={UserId}, Email={Email}, Username={Username}. " +
                    "Errors={Errors}.", appUser.Id, appUser.Email, appUser.UserName, result.Errors);
            else
                _logger.LogInformation(0004, "Successfully updated user account information. " +
                    "UserId={UserId}, Email={Email}, Username={Username}.",
                    appUser.Id, appUser.Email, appUser.UserName);
            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(2011, ex, "An error occurred while trying update the users account information. " +
                "UserId: {UserId}, Email: {Email}, Username: {Username}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}." 
                , appUser.Id, appUser.Email, appUser.UserName, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task LogOutUserAsync()
    {
        try
        {
            AppUser currentUser = await GetCurrentUserAsync();
            await _signInManager.SignOutAsync().ConfigureAwait(false);
            
            _logger.LogInformation(0005, "Successfully signed out user. " +
                "UserId={UserId}, Email={Email}, Username={Username}. ",
                currentUser.Id, currentUser.Email, currentUser.UserName);
        }
        catch (Exception ex)
        {
            _logger.LogError(2012, ex, "An error occurred while trying to log out the user. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<bool> DeleteUserAccountAsync(AppUser appUser)
    {
        try
        {
            var result = await _userManager.DeleteAsync(appUser);
            if (!result.Succeeded)
                _logger.LogWarning(1006, "User account could not be deleted. " +
                    "UserId={UserId}, Email={Email}, Username={Username}. " +
                    "Errors={Errors}.", appUser.Id, appUser.Email, appUser.UserName, result.Errors);
            else
                _logger.LogInformation(0006, "Successfully deleted user account. " +
                    "UserId={UserId}, Email={Email}, Username={Username}.",
                    appUser.Id, appUser.Email, appUser.UserName);

            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(2013, ex, "An error occurred while trying to delete the user account. " +
                "UserId: {UserId}, Email: {Email}, Username: {Username}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}."
                , appUser.Id, appUser.Email, appUser.UserName, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<string> CreateResetPasswordTokenAsync(AppUser appUser)
    {
        try
        {
            string passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(appUser);
            _logger.LogInformation(0007, "Successfully created password reset token. " +
                    "UserId={UserId}, Email={Email}, Username={Username}.",
                    appUser.Id, appUser.Email, appUser.UserName);

            return passwordResetToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(2014, ex, "An error occurred while trying to create reset account password token. " +
                "UserId: {UserId}, Email: {Email}, Username: {Username}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}."
                , appUser.Id, appUser.Email, appUser.UserName, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<bool> ResetPasswordAsync(string userId, string resetPasswordToken, string newPassword)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                _logger.LogWarning(1007, "Tried to reset account password of null user: " +
                    "UserId={UserId}, ResetPasswordToken={ResetPasswordToken}.", userId, resetPasswordToken);
                return false;
            }

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordToken, newPassword);

            if (!result.Succeeded)
            {
                _logger.LogWarning(1008, "User account password could not be reset. " +
                    "UserId={UserId}, ResetPasswordToken={ResetPasswordToken}. " +
                    "Errors={Errors}.", userId, resetPasswordToken, result.Errors);
                return false;
            }

            await _signInManager.SignInAsync(user, false);
            _logger.LogInformation(0008, "Successfully reset account password. " +
                    "UserId={UserId}, ResetPasswordToken={ResetPasswordToken}.",
                    userId, resetPasswordToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(2015, ex, "An error occurred while trying reset user's account password. " +
                "UserId: {UserId}. ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}."
                , userId, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<string> CreateChangeEmailTokenAsync(AppUser appUser, string newEmail)
    {
        try
        {
            string emailChangeToken = await _userManager.GenerateChangeEmailTokenAsync(appUser, newEmail);
            _logger.LogInformation(0009, "Successfully created email change token. " +
                    "UserId={UserId}, Email={Email}, Username={Username}, NewEmail={NewEmail}.",
                    appUser.Id, appUser.Email, appUser.UserName, newEmail);

            return emailChangeToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(2016, ex, "An error occurred while trying to create account email reset token. " + 
                "UserId: {UserId}, Email: {Email}, Username: {Username}, NewEmail, {NewEmail}. " + 
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", 
                appUser.Id, appUser.Email, appUser.UserName, newEmail, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<bool> ChangeEmailAsync(string userId, string changeEmailToken, string newEmail)
    {

        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning(1009, "Tried to change account email of null user: " +
                    "UserId={UserId}, ChangeEmailToken={ChangeEmailToken}, NewEmail={NewEmail}.", userId, changeEmailToken, newEmail);
                return false;
            }

            var strategy = _identityContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = _identityContext.Database.BeginTransaction())
                {
                    try
                    {
                        var result = await _userManager.ChangeEmailAsync(user, newEmail, changeEmailToken);
                        if (!result.Succeeded)
                        {
                            throw new Exception("Failed to change email.");
                        }

                        var externalLogins = await _userManager.GetLoginsAsync(user);

                        foreach (var externalLogin in externalLogins)
                        {                            
                            var removed = await _userManager.RemoveLoginAsync(user, externalLogin.LoginProvider, externalLogin.ProviderKey);
                            if (!removed.Succeeded)
                            {
                                throw new Exception("Failed to remove external login.");
                            }
                        }

                        await _signInManager.SignInAsync(user, false);
                        await transaction.CommitAsync();
                        _logger.LogInformation(0010, "Successfully changed user's email account. " +
                            "UserId={UserId}, ChangeEmailToken={ChangeEmailToken}, NewEmail={NewEmail}.",
                            userId, changeEmailToken, newEmail);

                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            });

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(2017, ex, "An error occurred while trying to change user email account. " +
                "UserId: {UserId}, NewEmail: {NewEmail}. ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.",
                userId, newEmail, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<bool> AddFriend(string userId, string friendId)
    {
        try
        {
            if (userId == friendId)
                return false;

            Friendship friendship = new () { UserId = userId, FriendId = friendId};
            await _identityContext.Friendships.AddAsync(friendship);
            await _identityContext.SaveChangesAsync();

            _identityContext.Entry(friendship).State = EntityState.Detached;
            
            _logger.LogInformation(0011, "Successfully added friend to user's account. " +
                "UserId={UserId}, FriendId={FriendId}.", userId, friendId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(2018, ex, "An error occurred while trying adding friend to user account. " +
                "UserId: {UserId}, FriendId: {FriendId}. ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.",
                userId, friendId, ex.Message, ex.StackTrace);

            return false;
        }
    }

    public async Task<bool> RemoveFriend(string userId, string friendId)
    {
        try
        {
            AppUser foundUser = await FindByUserIdAsync(userId);
            if (foundUser is null)
            {
                _logger.LogWarning(1010, "Tried to remove friend of null user. UserId={UserId}, FriendId={FriendId}.", userId, friendId);
                return false;
            }

            Friendship foundFriendship = foundUser.Friendships.FirstOrDefault(friendship => friendship.FriendId == friendId 
            || friendship.UserId == friendId)!;
            if (foundFriendship is null)
            {
                _logger.LogWarning(1011, "friend of user could not be found. UserId={UserId}, FriendId={FriendId}", userId, friendId);
                return false;
            }

            _identityContext.Friendships.Remove(foundFriendship);

            await _identityContext.SaveChangesAsync();

            _logger.LogInformation(0012, "Successfully removed friend from user's account. " +
                "UserId={UserId}, FriendId={FriendId}.", userId, friendId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(2019, ex, "An error occurred while trying removing friend from user account. " +
                "UserId: {UserId}, FriendId: {FriendId}. ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.",
            userId, friendId, ex.Message, ex.StackTrace);

            return false;
        }
    }

    public async Task<bool> UpdateSignalRConnectionIdOfUser(string userId, string connectionId)
    {
        try
        {
            AppUser appUser = await FindByUserIdAsync(userId);
            if (appUser is null)
                return false;

            appUser.SignalRConnectionId = connectionId;            
            var result = await _userManager.UpdateAsync(appUser);
            
            if (!result.Succeeded)
                _logger.LogWarning(1012, "signalR connection could not be established for user. " +
                    "UserId={UserId}, ConnectionId={ConnectionId}. Errors={Errors}.", userId, connectionId, result.Errors);
            else
                _logger.LogInformation(0013, "Successfully updated signalR connection of user. " +
                    "UserId={UserId}, ConnectionId={ConnectionId}.", userId, connectionId);
            
            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(2020, ex, "An error occurred while trying to update user's SignalR connection. " +
                "UserId: {UserId}, ConnectionId: {ConnectionId}. ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.",
            userId, connectionId, ex.Message, ex.StackTrace);
            
            throw;
        }
    }

    public async Task<bool> UpdateUserChatStatus(string userId, int? chatId)
    {
        try
        {
            AppUser appUser = await FindByUserIdAsync(userId);
            if (appUser is null)
                return false;

            appUser.InChatWithId = chatId;
            var result = await _userManager.UpdateAsync(appUser);

            if (!result.Succeeded)
                _logger.LogWarning(1013, "chat status of user could not be updated. " +
                    "UserId={UserId}, ChatId={ChatId}. Errors={Errors}.", userId, chatId, result.Errors);
            else
                _logger.LogInformation(0014, "Successfully updated chat status of user(which chat the user is currently connected to). " +
                    "UserId={UserId}, ChatId={ChatId}.", userId, chatId);

            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(2021, ex, "An error occurred while trying to update user's chat status(the chat they have connected to). " +
                "UserId: {UserId}, ChatId: {ChatId}. ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.",
                userId, chatId, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<IEnumerable<AuthenticationScheme>> GetExternalIdentityProvidersAsync()
    {
        try
        {
            return await _signInManager.GetExternalAuthenticationSchemesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(2022, ex, "An error occurred while trying to get the external identity providers. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public AuthenticationProperties GetExternalIdentityProvidersPropertiesAsync(string identityProviderName, string redirectUrl)
    {
        try
        {
            return _signInManager.ConfigureExternalAuthenticationProperties(identityProviderName, redirectUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(2023, ex, "An error occurred while trying to get the external identity provider's properties. " +
                "IdentityProviderName: {IdentityProviderName}, RedirectUrl: {RedirectUrl}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.",
            identityProviderName, redirectUrl, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<string> ExternalSignInUserAsync()
    {
        try
        {
            ExternalLoginInfo? loginInfo = await _signInManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
                return "login info of external identity provider was not received";

            var result = await _signInManager.ExternalLoginSignInAsync(loginInfo.LoginProvider, loginInfo.ProviderKey, isPersistent: false, bypassTwoFactor: false);
            if (result.Succeeded)
            {
                _logger.LogInformation(0015, "Successfully signed in user with external login provider.");
                return null!;
            }

            string email = loginInfo.Principal.FindFirstValue(ClaimTypes.Email)! ?? loginInfo.Principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
            string username = loginInfo.Principal.FindFirstValue(ClaimTypes.Email)! ?? loginInfo.Principal.FindFirstValue(ClaimTypes.Name)! + loginInfo.Principal.FindFirstValue(ClaimTypes.NameIdentifier)?.Substring(0, 5);
            //if the returned information does not contain email give up
            if (email == null)
                return "email info of external identity provider was not received";

            //if the user has a local account
            AppUser user = await FindByEmailAsync(email);
            if (user != null)
            {
                //activating the email here for edge case where a user tries to first create a local account does not activate it
                //and then continues with google
                user.EmailConfirmed = true;

                await _userManager.AddLoginAsync(user, loginInfo);
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation(0016, "Successfully linked external login to user local account and signed them in. " +
                    "Username={Username}, Email={Email}.", username, email);
                return null!;
            }

            //otherwise
            user = new AppUser() { UserName = username, Email = email, EmailConfirmed = true };
            await _userManager.CreateAsync(user);
            await _userManager.AddLoginAsync(user, loginInfo);
            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation(0017, "Successfully created a local account linked the external login to it and signed the user in. " +
                "Username={Username}, Email={Email}.", username, email);

            return null!;
        }
        catch (Exception ex)
        {
            _logger.LogError(2024, ex, "An error occurred while trying to sign in the user with external identity provider. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }
}

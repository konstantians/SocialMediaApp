using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Authentication;
using SocialMediaApp.SharedModels;
using System.Diagnostics.Eventing.Reader;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace SocialMediaApp.AuthenticationLibrary;

public class AuthenticationProcedures : IAuthenticationProcedures
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly AppIdentityDbContext _identityContext;

    public AuthenticationProcedures(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, AppIdentityDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _identityContext = context;
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
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task<AppUser> GetCurrentUserAsync()
    {
        try
        {
            AppUser currentUser = await _userManager.GetUserAsync(_signInManager.Context.User);
            return AddFriendshipsToUser(currentUser);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
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
            Console.WriteLine(ex);
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
            Console.WriteLine(ex);
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
            Console.WriteLine(ex);
            throw;
        }
    }

    private AppUser AddFriendshipsToUser(AppUser appUser)
    {
        if (appUser == null)
            return null;

        appUser.Friendships = _identityContext.Friendships
        .Where(friend => friend.UserId == appUser.Id || friend.FriendId == appUser.Id)
        .ToList();
        return appUser;
    }

    public async Task<(string, string)> RegisterUserAsync(AppUser appUser, string password,
        bool isPersistent)
    {
        try
        {
            var result = await _userManager.CreateAsync(appUser, password);
            string confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
            return (appUser.Id, confirmationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
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
                return false;
            }

            var result = await _userManager.ConfirmEmailAsync(user, confirmationToken);

            if (!result.Succeeded)
            {
                return false;
            }

            await _signInManager.SignInAsync(user, false);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
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
                return (false, "unknown");
            return (true, "nothing");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return (false, "unknown");
        }
    }

    public async Task<bool> SignInUserAsync(string username, string password, bool isPersistent)
    {
        try
        {
            var result = await _signInManager.PasswordSignInAsync(username, password, isPersistent, false);
            return result.Succeeded;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task<bool> CheckIfUserIsLoggedIn()
    {
        return await Task.Run(() => _signInManager.IsSignedIn(_signInManager.Context.User));
    }

    public async Task<bool> UpdateUserAccountAsync(AppUser appUser)
    {
        try
        {
            var result = await _userManager.UpdateAsync(appUser);
            return result.Succeeded;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task LogOutUserAsync()
    {
        try
        {
            await _signInManager.SignOutAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task<bool> DeleteUserAccountAsync(AppUser appUser)
    {
        try
        {
            var result = await _userManager.DeleteAsync(appUser);
            return result.Succeeded;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task<string> CreateResetPasswordTokenAsync(AppUser appUser)
    {
        try
        {
            return await _userManager.GeneratePasswordResetTokenAsync(appUser);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
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
                return false;
            }

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordToken, newPassword);

            if (!result.Succeeded)
            {
                return false;
            }

            await _signInManager.SignInAsync(user, false);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task<string> CreateChangeEmailTokenAsync(AppUser appUser, string newEmail)
    {
        try
        {
            return await _userManager.GenerateChangeEmailTokenAsync(appUser, newEmail);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
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
                            throw new Exception("Failed to change email.");

                        var externalLogins = await _userManager.GetLoginsAsync(user);

                        foreach (var externalLogin in externalLogins)
                        {                            
                            var removed = await _userManager.RemoveLoginAsync(user, externalLogin.LoginProvider, externalLogin.ProviderKey);
                            if (!removed.Succeeded)
                                throw new Exception("Failed to remove external login.");
                        }

                        await _signInManager.SignInAsync(user, false);
                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw ex;
                    }
                }
            });

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task<bool> AddFriend(string userId, string friendId)
    {
        try
        {
            if(userId == friendId)
                return false;

            Friendship friendship = new () { UserId = userId, FriendId = friendId};
            await _identityContext.Friendships.AddAsync(friendship);
            await _identityContext.SaveChangesAsync();

            _identityContext.Entry(friendship).State = EntityState.Detached;
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }

    public async Task<bool> RemoveFriend(string userId, string friendId)
    {
        try
        {
            AppUser foundUser = await FindByUserIdAsync(userId);
            if (foundUser is null)
                return false;

            Friendship foundFriendship = foundUser.Friendships.FirstOrDefault(friendship => friendship.FriendId == friendId 
            || friendship.UserId == friendId);
            if (foundFriendship is null)
                return false;

            _identityContext.Friendships.Remove(foundFriendship);

            await _identityContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
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
            return result.Succeeded;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
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
            return result.Succeeded;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task<IEnumerable<AuthenticationScheme>> GetExternalIdentityProvidersAsync()
    {
        return await _signInManager.GetExternalAuthenticationSchemesAsync();
    }

    public AuthenticationProperties GetExternalIdentityProvidersPropertiesAsync(string identityProviderName, string redirectUrl)
    {
        return _signInManager.ConfigureExternalAuthenticationProperties(identityProviderName, redirectUrl);
    }

    public async Task<string> ExternalSignInUserAsync()
    {
        ExternalLoginInfo loginInfo = await _signInManager.GetExternalLoginInfoAsync();
        if (loginInfo == null)
            return "login info of external identity provider was not received";

        var result = await _signInManager.ExternalLoginSignInAsync(loginInfo.LoginProvider, loginInfo.ProviderKey, isPersistent: false, bypassTwoFactor: false);
        if (result.Succeeded)
            return null!;

        string email = loginInfo.Principal.FindFirstValue(ClaimTypes.Email)! ?? loginInfo.Principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
        string username = loginInfo.Principal.FindFirstValue(ClaimTypes.Email)! ?? loginInfo.Principal.FindFirstValue(ClaimTypes.Surname)! + loginInfo.Principal.FindFirstValue(ClaimTypes.NameIdentifier)?.Substring(0, 5);
        //if the returned information does not contain email give up
        if (email == null)
            return "email info of external identity provider was not received";

        //if the user has a local account
        AppUser user = await FindByEmailAsync(email);
        if (user != null)
        {
            await _userManager.AddLoginAsync(user, loginInfo);
            await _signInManager.SignInAsync(user, isPersistent: false);
            return null!;
        }

        //otherwise
        user = new AppUser() { UserName = username, Email = email, EmailConfirmed = true};
        await _userManager.CreateAsync(user);
        await _userManager.AddLoginAsync(user, loginInfo);
        await _signInManager.SignInAsync(user, isPersistent: false);

        return null!;
    }
}

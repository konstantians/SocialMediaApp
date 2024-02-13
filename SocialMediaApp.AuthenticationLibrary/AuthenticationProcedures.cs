using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.SharedModels;

namespace SocialMediaApp.AuthenticationLibrary;

public class AuthenticationProcedures : IAuthenticationProcedures
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    public AuthenticationProcedures(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<List<AppUser>> GetUsersAsync()
    {
        try
        {
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
            return await _userManager.GetUserAsync(_signInManager.Context.User);
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
            return await _userManager.FindByIdAsync(userId);
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
            return await _userManager.FindByNameAsync(username);
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
            return await _userManager.FindByEmailAsync(email);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
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
            var result = await _userManager.ChangePasswordAsync(appUser, currentPassword, newPassword);
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
            if (user is null)
            {
                return false;
            }

            var result = await _userManager.ChangeEmailAsync(user, newEmail, changeEmailToken);

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
}

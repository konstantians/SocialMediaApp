using SocialMediaApp.SharedModels;

namespace SocialMediaApp.AuthenticationLibrary
{
    public interface IAuthenticationProcedures
    {
        Task<bool> AddFriend(string userId, string friendId);
        Task<bool> ChangeEmailAsync(string userId, string changeEmailToken, string newEmail);
        Task<(bool, string)> ChangePasswordAsync(AppUser appUser, string currentPassword, string newPassword);
        Task<bool> CheckIfUserIsLoggedIn();
        Task<bool> ConfirmEmailAsync(string userId, string confirmationToken);
        Task<string> CreateChangeEmailTokenAsync(AppUser appUser, string newEmail);
        Task<string> CreateResetPasswordTokenAsync(AppUser appUser);
        Task<bool> DeleteUserAccountAsync(AppUser appUser);
        Task<AppUser> FindByEmailAsync(string email);
        Task<AppUser> FindByUserIdAsync(string userId);
        Task<AppUser> FindByUsernameAsync(string username);
        Task<AppUser> GetCurrentUserAsync();
        Task<List<AppUser>> GetUsersAsync();
        Task LogOutUserAsync();
        Task<(string, string)> RegisterUserAsync(AppUser appUser, string password, bool isPersistent);
        Task<bool> RemoveFriend(string userId, string friendId);
        Task<bool> ResetPasswordAsync(string userId, string resetPasswordToken, string newPassword);
        Task<bool> SignInUserAsync(string username, string password, bool isPersistent);
        Task<bool> UpdateUserAccountAsync(AppUser appUser);
    }
}
using SocialMediaApp.SharedModels;

namespace SocialMediaApp.MVC.Models.EditAccountModels;

public class EditAccountModel
{
    public List<AppUser> UsersFriends { get; set; } = new List<AppUser>();
    public AccountBasicSettingsViewModel AccountBasicSettingsViewModel { get; set; } = new();
    public ChangePasswordModel ChangePasswordModel { get; set; } = new();
    public ChangeEmailModel ChangeEmailModel { get; set; } = new();
}

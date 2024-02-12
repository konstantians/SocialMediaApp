using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.MVC.Models.EditAccountModels;

public class ChangeEmailModel
{
    [Required(ErrorMessage = "This field is required")]
    public string? OldEmail { get; set; }

    [Required(ErrorMessage = "This field is required")]
    public string? NewEmail { get; set; }

}

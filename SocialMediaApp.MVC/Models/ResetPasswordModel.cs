using SocialMediaApp.MVC.CustomValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.MVC.Models;

public class ResetPasswordModel
{
    [Required]
    public string? UserId { get; set; }

    [Required]
    public string? Token { get; set; }

    [Required(ErrorMessage = "This field is required")]
    [ComparePassword("Password", ErrorMessage = "The passwords do not match.")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "This field is required")]
    [ComparePassword("Password", ErrorMessage = "The passwords do not match.")]
    public string? ConfirmPassword { get; set; }

}

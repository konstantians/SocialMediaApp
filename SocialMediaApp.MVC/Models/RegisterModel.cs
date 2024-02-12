using SocialMediaApp.MVC.CustomValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.MVC.Models;

public class RegisterModel
{
    [Required(ErrorMessage = "This field is required")]
    public string? Username { get; set; }

    [Required(ErrorMessage = "This field is required")]
    [ComparePassword("Password", ErrorMessage = "The passwords do not match.")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "This field is required")]
    [ComparePassword("Password", ErrorMessage = "The passwords do not match.")]
    [Display(Name = "Repeat Password")]
    public string? RepeatPassword { get; set; }

    [Required(ErrorMessage = "This field is required")]
    [EmailAddress]
    public string? Email { get; set; }

    [Required(ErrorMessage = "This field is required")]
    [Phone]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

}

using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.MVC.Models.IndexModels;

public class EditPostModel : CreatePostModel
{
    [Required]
    public int PostId { get; set; }
}

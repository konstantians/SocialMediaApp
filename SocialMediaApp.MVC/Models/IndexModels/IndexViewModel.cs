using SocialMediaApp.SharedModels;

namespace SocialMediaApp.MVC.Models.IndexModels;

public class IndexViewModel
{
    public List<Post> Posts { get; set; } = new List<Post>();
    public CreatePostModel CreatePostModel { get; set; }
    public EditPostModel EditPostModel { get; set; }
}

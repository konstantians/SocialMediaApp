using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialMediaApp.SharedModels;

public class Post
{
    public int Id { get; set; }
    public DateTime SentAt { get; set; }
    [Required]
    [StringLength(40, MinimumLength = 1, ErrorMessage = "The Post Title Can Not Exceed 40 Characters")]
    public string? Title { get; set; }
    [Required]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "The Post Content Can Not Exceed 500 Characters")]
    public string? Content { get; set; }

    [NotMapped]
    public AppUser? AppUser { get; set; }
    public string UserId { get; set; }
    public List<PostVote> PostVotes { get; set; } = new List<PostVote>();
}

namespace SocialMediaApp.SharedModels;

public class PostVote
{
    public int Id { get; set; }
    public bool IsPositive { get; set; }
    public int PostId { get; set; }
    public Post? Post { get; set; }
}

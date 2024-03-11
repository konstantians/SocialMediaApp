using System.ComponentModel.DataAnnotations.Schema;

namespace SocialMediaApp.SharedModels;

public class MessageStatus
{
    public int Id { get; set; }
    public bool IsSeen { get; set; }

    public Message Message { get; set; }
    public int MessageId { get; set; }
    
    public string? UserId { get; set; }
    [NotMapped]
    public AppUser UserOfMessageStatus { get; set; }
}

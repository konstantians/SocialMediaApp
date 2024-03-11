using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialMediaApp.SharedModels;

public class Message
{
    public int Id { get; set; }
    public DateTime SentAt { get; set; }

    [StringLength(500, MinimumLength = 1, ErrorMessage = "The Message Can Not Exceed 500 Characters")]
    public string? Content { get; set; }
    public List<MessageStatus> MessageStatuses { get; set; } = new List<MessageStatus>();
    public int ChatId { get; set; }
    public Chat? Chat { get; set; }

    public string? UserId { get; set; }
    [NotMapped]
    public AppUser MessageAuthor { get; set; }

    public List<Notification> Notifications { get; set; } = new List<Notification>();
}

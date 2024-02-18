using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.SharedModels;

public class Message
{
    public int Id { get; set; }
    public DateTime SentAt { get; set; }

    [StringLength(500, MinimumLength = 1, ErrorMessage = "The Message Can Not Exceed 500 Characters")]
    public string? Content { get; set; }
    public bool IsSeen { get; set; }

    public int ChatId { get; set; }
    public Chat? Chat { get; set; }

    public Notification? Notification { get; set; }
}

using System.ComponentModel.DataAnnotations.Schema;

namespace SocialMediaApp.SharedModels;

public class Notification
{
    public int Id { get; set; }
    public DateTime SentAt { get; set; }
    public bool FriendRequestAccepted { get; set; }
    public bool FriendRequestRejected { get; set; }
    public bool NewFriendRequest { get; set; }

    [NotMapped]
    public AppUser Sender { get; set; }
    public string FromUserId { get; set; }

    [NotMapped]
    public AppUser Recepient { get; set; }
    public string ToUserId { get; set; }

    public int MessageId { get; set; }
    public Message? Message { get; set; }
}

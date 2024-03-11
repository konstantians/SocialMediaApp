using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialMediaApp.SharedModels;

public class AppUser : IdentityUser
{
    public List<Friendship> Friendships { get; set; } = new List<Friendship>();

    [NotMapped]
    public List<ChatsUsers> ChatsUsers { get; set; } = new List<ChatsUsers>();
    [NotMapped]
    public List<Notification> Notifications { get; set; } = new List<Notification>();
    [NotMapped]
    public List<Post> Posts { get; set; } = new List<Post>();
    public string? SignalRConnectionId { get; set; }
    public int? InChatWithId { get; set; }
}

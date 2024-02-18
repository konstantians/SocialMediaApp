using System.ComponentModel.DataAnnotations.Schema;

namespace SocialMediaApp.SharedModels;

public class ChatsUsers
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    [NotMapped]
    public AppUser? AppUser { get; set; }
    public int ChatId { get; set; }
    public Chat? Chat { get; set; }
}

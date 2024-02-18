namespace SocialMediaApp.SharedModels;

public class Chat
{
    public int Id { get; set; }
    public List<ChatsUsers> ChatsUsers { get; set; } = new List<ChatsUsers>();
    public List<Message> Messages { get; set; } = new List<Message>();
}

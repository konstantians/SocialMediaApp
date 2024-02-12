namespace CinemaApplication.EmailServiceLibrary
{
    public interface IEmailService
    {
        Task<bool> SendContactFormEmailAsync(string emailSender, string title, string body);
        Task<bool> SendEmailAsync(string emailReceiver, string title, string body);
        Task<bool> SendEmailAsync(string emailSender, string emailReceiver, string title, string body);
    }
}
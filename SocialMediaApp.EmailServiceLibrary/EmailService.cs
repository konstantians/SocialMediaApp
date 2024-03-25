namespace SocialMediaApp.EmailServiceLibrary;

using System.Net;
using System.Net.Mail;
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

/// <summary>
/// class that handles the email functionality of the app
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger = null!)
    {
        _configuration = configuration;
        _logger = logger ?? NullLogger<EmailService>.Instance;
    }

    /// <summary>
    /// This method sends an email using the emailSender string parameter to the given email, which is given by
    /// the emailReceiver string parameter. This method should only be used in case you either want to hardcode
    /// the defauly sender email("kinnaskonstantinos0@gmail.com") or you know what you are doing with the other 
    /// email you want to send the email. If you are not certain on what to do just use the simplified overload
    /// of the SendEmail method, which does not have a parameter for the emailSender(it is hardcoded).
    /// </summary>
    /// <param name="emailSender">The email account which will send the email</param>
    /// <param name="emailReceiver">The email account which will receive the email</param>
    /// <param name="title">The title of the email</param>
    /// <param name="body">The context of the email</param>
    /// <returns>Confirmation on whether or not the email was send successfully</returns>
    public async Task<bool> SendEmailAsync(string emailSender, string emailReceiver, string title, string body)
    {
        SmtpSettings smtpSettings = _configuration.GetSection("SmtpSettings").Get<SmtpSettings>();

        using var message = new MailMessage(emailSender, emailReceiver);

        message.Subject = title;
        message.Body = body;
        message.IsBodyHtml = false;

        using var smtpClient = new SmtpClient(smtpSettings.Host, smtpSettings.Port);
        smtpClient.EnableSsl = smtpSettings.EnableSsl;
        smtpClient.Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password);

        try
        {
            await smtpClient.SendMailAsync(message);
            _logger.LogError(0600, "Successfully sent email. " +
                "EmailSender:{EmailSender}, EmailReceiver:{EmailReceiver}, Title:{Title}, Body:{Body}",
                emailSender, emailReceiver, title, body);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(2600, ex, "An error occurred while trying to send email. " +
                "EmailSender:{EmailSender}, EmailReceiver:{EmailReceiver}, Title:{Title}, Body:{Body}" +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.",
                emailSender, emailReceiver, title, body, ex.Message, ex.StackTrace);
            return false;
        }
    }

    /// <summary>
    /// This method sends an email to the given email, which is given by the emailReceiver string parameter. This method
    /// is a simplified version of the other SendEmail overload, which also takes as an extra parameter the email sender. If
    /// you do not know, which to use you should use this one, because the hardcoded email this method is using has already been
    /// configured and it works correctly. In conclusion this method should be the default method for sending emails.
    /// </summary>
    /// <param name="emailReceiver">The email account which will receive the email</param>
    /// <param name="title">The title of the email</param>
    /// <param name="body">The context of the email</param>
    /// <returns>Confirmation on whether or not the email was send successfully</returns>
    public async Task<bool> SendEmailAsync(string emailReceiver, string title, string body)
    {
        SmtpSettings? smtpSettings = _configuration.GetSection("SmtpSettings").Get<SmtpSettings>();

        using var message = new MailMessage("kinnaskonstantinos0@gmail.com", emailReceiver);

        message.Subject = title;
        message.Body = body;
        message.IsBodyHtml = false;

        using var smtpClient = new SmtpClient(smtpSettings.Host, smtpSettings.Port);
        smtpClient.EnableSsl = smtpSettings.EnableSsl;
        smtpClient.Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password);

        try
        {
            await smtpClient.SendMailAsync(message);
            _logger.LogError(0601, "Successfully sent email. EmailReceiver:{EmailReceiver}, Title:{Title}, Body:{Body}",
                emailReceiver, title, body);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(2601, ex, "An error occurred while trying to send email. " +
                "EmailReceiver:{EmailReceiver}, Title:{Title}, Body:{Body}" +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.",
                emailReceiver, title, body, ex.Message, ex.StackTrace);
            return false;
        }
    }

    /// <summary>
    /// used to send messages from kinnaskonstantinos0@gmail.com to kinnaskonstantinos0@gmail.com adding the email of the user
    /// in the body of the email. The reason why this has to be done like that is because the application can not force the user
    /// to send an email to our email, so this is the simplest way to make it work.
    /// </summary>
    /// <param name="emailSender">The email of the sender which will be passed into the body of the email</param>
    /// <param name="title">The title of the contact form message</param>
    /// <param name="body">The context of the contact form message</param>
    /// <returns>Confirmation on whether or not the email was send successfully</returns>
    public async Task<bool> SendContactFormEmailAsync(string emailSender, string title, string body)
    {
        SmtpSettings? smtpSettings = _configuration.GetSection("SmtpSettings").Get<SmtpSettings>();

        using var message = new MailMessage("kinnaskonstantinos0@gmail.com", "kinnaskonstantinos0@gmail.com");
        message.Subject = title;
        message.Body = $"From: {emailSender}\n\n{body}";
        message.IsBodyHtml = false;

        using var smtpClient = new SmtpClient(smtpSettings.Host, smtpSettings.Port);
        smtpClient.EnableSsl = smtpSettings.EnableSsl;
        smtpClient.Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password);

        try
        {
            await smtpClient.SendMailAsync(message);
            _logger.LogError(0602, "Successfully sent contact form email. EmailSender:{EmailSender}, Title:{Title}, Body:{Body}",
                emailSender, title, body);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(2602, ex, "An error occurred while trying to send email from contact form. " +
                "EmailSender:{EmailSender}, Title:{Title}, Body:{Body}" +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.",
                emailSender, title, body, ex.Message, ex.StackTrace);
            return false;
        }
    }
}






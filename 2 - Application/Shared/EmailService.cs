using AvaliaRBI._3___Domain.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace AvaliaRBI._2___Application.Shared;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string content, TextFormat format);
}

public class EmailService : IEmailService
{
    private readonly AppSettings _appSettings;

    public EmailService()
    {
        _appSettings = new AppSettings();
    }

    public async Task SendEmailAsync(string to, string subject, string content, TextFormat format)
    {
        try
        {
            // create message
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_appSettings.EmailFrom));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart(format) { Text = content };

            // send email
            using var smtp = new SmtpClient();
            smtp.Connect(_appSettings.SmtpHost, _appSettings.SmtpPort, SecureSocketOptions.StartTls);
            smtp.Authenticate(_appSettings.SmtpUser, _appSettings.SmtpPass);
            await smtp.SendAsync(email);
            smtp.Disconnect(true);
        }
        catch (Exception)
        {

            throw;
        }

    }

    public async Task SendEmailAsync(string to, string subject, string content, TextFormat format, _3___Domain.Models.FileInfo file)
    {
        var tempFilePath = Path.Combine(Path.GetTempPath(), file.FileName);
        File.WriteAllBytes(tempFilePath, file.FileBytes);

        try
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_appSettings.EmailFrom));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;

            var body = new TextPart(format) { Text = content };

            var stream = File.OpenRead(tempFilePath);
            var attachment = new MimePart("application", "pdf")
            {
                Content = new MimeContent(stream, ContentEncoding.Default),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = file.FileName
            };

            var multipart = new Multipart("mixed")
            {
                body,
                attachment
            };

            email.Body = multipart;

            using var smtp = new SmtpClient();
            smtp.Connect(_appSettings.SmtpHost, _appSettings.SmtpPort, SecureSocketOptions.StartTls);
            smtp.Authenticate(_appSettings.SmtpUser, _appSettings.SmtpPass);
            await smtp.SendAsync(email);
            smtp.Disconnect(true);

            stream.Close();
        }
        finally
        {
            File.Delete(tempFilePath);
        }
    }

    public async Task SendEmailAsync(EmailModel emailModel)
    {
        var tempFilePath = Path.Combine(Path.GetTempPath(), emailModel.File.FileName);
        File.WriteAllBytes(tempFilePath, emailModel.File.FileBytes);

        try
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_appSettings.EmailFrom));
            email.To.Add(MailboxAddress.Parse(emailModel.ToEmail));
            email.Subject = emailModel.Subject;

            var body = new TextPart(emailModel.Format) { Text = emailModel.Content };

            var stream = File.OpenRead(tempFilePath);
            var attachment = new MimePart("application", emailModel.File.Extension)
            {
                Content = new MimeContent(stream, ContentEncoding.Default),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = emailModel.File.FileName + "." + emailModel.File.Extension
            };

            var multipart = new Multipart("mixed")
            {
                body,
                attachment
            };

            email.Body = multipart;

            using var smtp = new SmtpClient();
            smtp.Connect(_appSettings.SmtpHost, _appSettings.SmtpPort, SecureSocketOptions.StartTls);
            smtp.Authenticate(_appSettings.SmtpUser, _appSettings.SmtpPass);
            await smtp.SendAsync(email);
            smtp.Disconnect(true);

            stream.Close();
        }
        finally
        {
            File.Delete(tempFilePath);
        }
    }
}

public class AppSettings
{
    public string SmtpUser { get; set; } = "rbi.papeis.teste@gmail.com";
    public string EmailFrom { get; set; } = "rbi.papeis.teste@gmail.com";
    public string SmtpPass { get; set; } = "wvcn nand brwm uzng";
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
}


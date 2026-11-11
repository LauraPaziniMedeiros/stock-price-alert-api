namespace StockPriceAlert.Services;

using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using StockPriceAlert.Configuration;

/// <summary>
/// Dispatches email notifications using the MailKit SMTP client.
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(EmailSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public async Task SendAlertAsync(string subject, string body, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        if (string.IsNullOrWhiteSpace(_settings.SmtpServer) ||
            string.IsNullOrWhiteSpace(_settings.SenderEmail) ||
            string.IsNullOrWhiteSpace(_settings.RecipientEmail))
        {
            Console.Error.WriteLine(
                "[EmailService] Cannot send email: SMTP configuration is incomplete. " +
                "Please configure SenderEmail, RecipientEmail, and SmtpServer in appsettings.local.json.");
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(new MailboxAddress("", _settings.RecipientEmail));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.SmtpServer, _settings.Port, SecureSocketOptions.Auto, cancellationToken);

            if (!string.IsNullOrWhiteSpace(_settings.SenderPassword))
            {
                await client.AuthenticateAsync(_settings.SenderEmail, _settings.SenderPassword, cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(quit: true, cancellationToken);

            Console.WriteLine($"[EmailService] Alert email successfully sent to '{_settings.RecipientEmail}'.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[EmailService] Failed to send alert email: {ex.Message}");
        }
    }
}

namespace StockPriceAlert.Services;

/// <summary>
/// Defines the contract for dispatching email alert notifications.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email alert with the specified subject and body.
    /// </summary>
    /// <param name="subject">The subject line of the email.</param>
    /// <param name="body">The plain text body content of the email.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendAlertAsync(string subject, string body, CancellationToken cancellationToken = default);
}

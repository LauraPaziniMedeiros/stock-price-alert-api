namespace StockPriceAlert.Configuration;

/// <summary>
/// Strongly-typed POCO that maps to the "EmailSettings" section in appsettings.json.
/// Used by the Configuration Binder to convert raw JSON key-value pairs into typed properties.
/// </summary>
public class EmailSettings
{
    public string SmtpServer { get; set; } = string.Empty;
    public int Port { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderPassword { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
}

namespace StockPriceAlert.Configuration;

/// <summary>
/// Strongly-typed POCO that maps to the "StockApi" section in appsettings.json.
/// Holds the base URL, authentication token, and polling interval for the quote API.
/// </summary>
public class StockApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int IntervalSeconds { get; set; } = 60;
}

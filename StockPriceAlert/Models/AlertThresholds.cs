namespace StockPriceAlert.Models;

/// <summary>
/// Encapsulates the CLI-provided monitoring parameters:
/// which stock to watch and at what prices to trigger alerts.
/// </summary>
/// <param name="Symbol">The stock ticker symbol to monitor, e.g. "PETR4".</param>
/// <param name="SellTarget">Send a sell alert when the price rises above this value.</param>
/// <param name="BuyTarget">Send a buy alert when the price drops below this value.</param>
public record AlertThresholds(string Symbol, decimal SellTarget, decimal BuyTarget);

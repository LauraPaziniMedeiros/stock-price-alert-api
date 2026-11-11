namespace StockPriceAlert.Services;

using StockPriceAlert.Models;

/// <summary>
/// Defines the contract for retrieving real-time stock quotes.
/// </summary>
public interface IStockService
{
    /// <summary>
    /// Fetches the latest stock quote for the specified ticker symbol.
    /// </summary>
    /// <param name="symbol">The stock ticker symbol (e.g., "PETR4").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="StockQuote"/> if retrieved successfully; otherwise, <see langword="null"/>.</returns>
    Task<StockQuote?> GetQuoteAsync(string symbol, CancellationToken cancellationToken = default);
}

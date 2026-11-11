namespace StockPriceAlert.Models;

/// <summary>
/// Immutable data transfer object (DTO) representing a stock price
/// retrieved from an external quote API.
/// 
/// Uses C# "record" syntax, which automatically provides:
///   - Immutability (properties are init-only)
///   - Value-based equality (two StockQuotes with the same data are considered equal)
///   - A readable ToString() output
/// </summary>
/// <param name="Symbol">The stock ticker symbol, e.g. "PETR4".</param>
/// <param name="CurrentPrice">The latest market price in BRL.</param>
/// <param name="RetrievedAt">The timestamp when this quote was fetched.</param>
public record StockQuote(string Symbol, decimal CurrentPrice, DateTime RetrievedAt);

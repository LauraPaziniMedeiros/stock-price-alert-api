namespace StockPriceAlert.Services;

using System.Net.Http.Json;
using System.Text.Json.Serialization;
using StockPriceAlert.Configuration;
using StockPriceAlert.Models;

/// <summary>
/// Fetches stock quotes from the Brapi API (https://brapi.dev).
/// </summary>
public class BrapiStockService : IStockService
{
    private readonly HttpClient _httpClient;
    private readonly StockApiSettings _settings;

    public BrapiStockService(HttpClient httpClient, StockApiSettings settings)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public async Task<StockQuote?> GetQuoteAsync(string symbol, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        try
        {
            var baseUrl = _settings.BaseUrl.TrimEnd('/');
            var formattedSymbol = Uri.EscapeDataString(symbol.Trim().ToUpperInvariant());
            var url = $"{baseUrl}/{formattedSymbol}";

            if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                url += $"?token={Uri.EscapeDataString(_settings.ApiKey)}";
            }

            using var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                Console.Error.WriteLine($"[StockService] Failed to fetch quote for '{symbol}'. HTTP {(int)response.StatusCode}");
                return null;
            }

            var apiData = await response.Content.ReadFromJsonAsync<BrapiResponse>(cancellationToken: cancellationToken);
            var firstResult = apiData?.Results?.FirstOrDefault();

            if (firstResult?.RegularMarketPrice is null)
            {
                Console.Error.WriteLine($"[StockService] No price data returned for '{symbol}'.");
                return null;
            }

            return new StockQuote(
                Symbol: firstResult.Symbol ?? symbol.ToUpperInvariant(),
                CurrentPrice: firstResult.RegularMarketPrice.Value,
                RetrievedAt: DateTime.UtcNow
            );
        }
        catch (HttpRequestException ex)
        {
            Console.Error.WriteLine($"[StockService] Network error fetching quote for '{symbol}': {ex.Message}");
            return null;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[StockService] Unexpected error processing quote for '{symbol}': {ex.Message}");
            return null;
        }
    }

    private sealed class BrapiResponse
    {
        [JsonPropertyName("results")]
        public List<BrapiResult>? Results { get; set; }
    }

    private sealed class BrapiResult
    {
        [JsonPropertyName("symbol")]
        public string? Symbol { get; set; }

        [JsonPropertyName("regularMarketPrice")]
        public decimal? RegularMarketPrice { get; set; }
    }
}

namespace StockPriceAlert.Services;

using System.Globalization;
using StockPriceAlert.Models;

/// <summary>
/// Core engine that monitors stock prices, evaluates threshold criteria,
/// and triggers alert notifications when targets are breached.
/// </summary>
public class StockMonitorEngine
{
    private readonly IStockService _stockService;
    private readonly IEmailService _emailService;
    private readonly AlertThresholds _thresholds;
    private AlertState _lastAlertState = AlertState.None;

    public enum AlertState
    {
        None,
        SellAlertTriggered,
        BuyAlertTriggered
    }

    /// <summary>
    /// Current alert state of the engine.
    /// </summary>
    public AlertState CurrentAlertState => _lastAlertState;

    public StockMonitorEngine(
        IStockService stockService,
        IEmailService emailService,
        AlertThresholds thresholds)
    {
        _stockService = stockService ?? throw new ArgumentNullException(nameof(stockService));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _thresholds = thresholds ?? throw new ArgumentNullException(nameof(thresholds));
    }

    /// <summary>
    /// Fetches the latest price from the stock service and evaluates it against target thresholds.
    /// </summary>
    public async Task EvaluateOnceAsync(CancellationToken cancellationToken = default)
    {
        var quote = await _stockService.GetQuoteAsync(_thresholds.Symbol, cancellationToken);
        if (quote is null)
        {
            return;
        }

        await EvaluateQuoteAsync(quote, cancellationToken);
    }

    /// <summary>
    /// Evaluates a given quote against thresholds and dispatches email alerts if boundaries are crossed.
    /// </summary>
    public async Task EvaluateQuoteAsync(StockQuote quote, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(quote);

        var ptBr = CultureInfo.GetCultureInfo("pt-BR");
        var priceFormatted = quote.CurrentPrice.ToString("C2", ptBr);
        var sellFormatted = _thresholds.SellTarget.ToString("C2", ptBr);
        var buyFormatted = _thresholds.BuyTarget.ToString("C2", ptBr);

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {_thresholds.Symbol}: {priceFormatted} (Sell Target: {sellFormatted} | Buy Target: {buyFormatted})");

        if (quote.CurrentPrice > _thresholds.SellTarget)
        {
            if (_lastAlertState != AlertState.SellAlertTriggered)
            {
                var subject = $"[SELL Alert] {_thresholds.Symbol} reached {priceFormatted}";
                var body = $"Asset: {_thresholds.Symbol}\n" +
                           $"Current Price: {priceFormatted}\n" +
                           $"Sell Reference Target: {sellFormatted}\n" +
                           $"Timestamp: {quote.RetrievedAt:yyyy-MM-dd HH:mm:ss} UTC\n\n" +
                           $"Recommendation: Consider SELLING as the current price has risen above your target.";

                await _emailService.SendAlertAsync(subject, body, cancellationToken);
                _lastAlertState = AlertState.SellAlertTriggered;
            }
            else
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {_thresholds.Symbol} remains above sell target. Alert already sent, skipping duplicate email.");
            }
        }
        else if (quote.CurrentPrice < _thresholds.BuyTarget)
        {
            if (_lastAlertState != AlertState.BuyAlertTriggered)
            {
                var subject = $"[BUY Alert] {_thresholds.Symbol} dropped to {priceFormatted}";
                var body = $"Asset: {_thresholds.Symbol}\n" +
                           $"Current Price: {priceFormatted}\n" +
                           $"Buy Reference Target: {buyFormatted}\n" +
                           $"Timestamp: {quote.RetrievedAt:yyyy-MM-dd HH:mm:ss} UTC\n\n" +
                           $"Recommendation: Consider BUYING as the current price has fallen below your target.";

                await _emailService.SendAlertAsync(subject, body, cancellationToken);
                _lastAlertState = AlertState.BuyAlertTriggered;
            }
            else
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {_thresholds.Symbol} remains below buy target. Alert already sent, skipping duplicate email.");
            }
        }
        else
        {
            if (_lastAlertState != AlertState.None)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {_thresholds.Symbol} has returned to normal range ({buyFormatted} - {sellFormatted}). Resetting alert state.");
                _lastAlertState = AlertState.None;
            }
        }
    }
}

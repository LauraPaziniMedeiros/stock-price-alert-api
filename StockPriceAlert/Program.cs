using System.Globalization;
using Microsoft.Extensions.Configuration;
using StockPriceAlert.Configuration;
using StockPriceAlert.Models;
using StockPriceAlert.Services;

// Validate command-line arguments: <SYMBOL> <SELL_TARGET> <BUY_TARGET>
if (args.Length != 3)
{
    Console.Error.WriteLine("Error: Incorrect number of arguments.");
    Console.WriteLine("Usage: dotnet run -- <SYMBOL> <SELL_TARGET> <BUY_TARGET>");
    Console.WriteLine("Example: dotnet run -- PETR4 22.67 22.59");
    return 1;
}

var symbol = args[0].Trim().ToUpperInvariant();

if (!decimal.TryParse(args[1], NumberStyles.Number, CultureInfo.InvariantCulture, out var sellTarget) &&
    !decimal.TryParse(args[1], NumberStyles.Number, CultureInfo.CurrentCulture, out sellTarget))
{
    Console.Error.WriteLine($"Error: Invalid sell target '{args[1]}'. Must be a valid numeric price.");
    return 1;
}

if (!decimal.TryParse(args[2], NumberStyles.Number, CultureInfo.InvariantCulture, out var buyTarget) &&
    !decimal.TryParse(args[2], NumberStyles.Number, CultureInfo.CurrentCulture, out buyTarget))
{
    Console.Error.WriteLine($"Error: Invalid buy target '{args[2]}'. Must be a valid numeric price.");
    return 1;
}

if (buyTarget >= sellTarget)
{
    Console.Error.WriteLine($"Error: Buy target ({buyTarget:F2}) must be lower than sell target ({sellTarget:F2}).");
    return 1;
}

// Load Configuration (appsettings.json and optional appsettings.local.json)
var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
    .Build();

var emailSettings = config.GetSection("EmailSettings").Get<EmailSettings>() ?? new EmailSettings();
var stockApiSettings = config.GetSection("StockApi").Get<StockApiSettings>() ?? new StockApiSettings();

// Shutdown on Ctrl+C
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cts.Cancel();
    Console.WriteLine("\n[Shutdown] Cancellation requested. Stopping monitor...");
};

// Initialize Services and Engine
using var httpClient = new HttpClient();
IStockService stockService = new BrapiStockService(httpClient, stockApiSettings);
IEmailService emailService = new EmailService(emailSettings);
var thresholds = new AlertThresholds(symbol, sellTarget, buyTarget);
var engine = new StockMonitorEngine(stockService, emailService, thresholds);

var ptBr = CultureInfo.GetCultureInfo("pt-BR");
var pollingInterval = TimeSpan.FromSeconds(Math.Max(5, stockApiSettings.IntervalSeconds));

Console.WriteLine("==================================================");
Console.WriteLine($" Monitoring:        {symbol}");
Console.WriteLine($" Sell Alert Target: >= {sellTarget.ToString("C2", ptBr)}");
Console.WriteLine($" Buy Alert Target:  <= {buyTarget.ToString("C2", ptBr)}");
Console.WriteLine($" Polling Interval:  {pollingInterval.TotalSeconds} seconds");
Console.WriteLine(" Press Ctrl+C to terminate the application.");
Console.WriteLine("==================================================");

try
{
    // Immediate initial evaluation on startup
    await engine.EvaluateOnceAsync(cts.Token);

    // Periodic monitoring loop
    using var timer = new PeriodicTimer(pollingInterval);
    while (!cts.Token.IsCancellationRequested && await timer.WaitForNextTickAsync(cts.Token))
    {
        await engine.EvaluateOnceAsync(cts.Token);
    }
}
catch (OperationCanceledException)
{
    // Exit when Ctrl+C is triggered
}

Console.WriteLine("Stock monitor stopped. Goodbye!");
return 0;

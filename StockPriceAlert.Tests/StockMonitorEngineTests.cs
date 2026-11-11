namespace StockPriceAlert.Tests;

using Moq;
using StockPriceAlert.Models;
using StockPriceAlert.Services;
using Xunit;

public class StockMonitorEngineTests
{
    private readonly Mock<IStockService> _stockServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly AlertThresholds _thresholds;

    public StockMonitorEngineTests()
    {
        _stockServiceMock = new Mock<IStockService>();
        _emailServiceMock = new Mock<IEmailService>();
        // Buy target: R$ 20.00, Sell target: R$ 25.00
        _thresholds = new AlertThresholds("PETR4", 25.00m, 20.00m);
    }

    [Fact]
    public async Task EvaluateQuoteAsync_WhenPriceAboveSellTarget_SendsSellEmailAndUpdatesState()
    {
        // Arrange
        var engine = new StockMonitorEngine(_stockServiceMock.Object, _emailServiceMock.Object, _thresholds);
        var highQuote = new StockQuote("PETR4", 26.50m, DateTime.UtcNow);

        // Act
        await engine.EvaluateQuoteAsync(highQuote);

        // Assert
        _emailServiceMock.Verify(
            email => email.SendAlertAsync(
                It.Is<string>(subject => subject.Contains("SELL")),
                It.Is<string>(body => body.Contains("PETR4") && body.Contains("SELLING")),
                It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.Equal(StockMonitorEngine.AlertState.SellAlertTriggered, engine.CurrentAlertState);
    }

    [Fact]
    public async Task EvaluateQuoteAsync_WhenPriceRemainsAboveSellTarget_DoesNotSendDuplicateEmail()
    {
        // Arrange
        var engine = new StockMonitorEngine(_stockServiceMock.Object, _emailServiceMock.Object, _thresholds);
        var quote1 = new StockQuote("PETR4", 26.00m, DateTime.UtcNow);
        var quote2 = new StockQuote("PETR4", 27.00m, DateTime.UtcNow);

        // Act - First spike triggers alert
        await engine.EvaluateQuoteAsync(quote1);

        // Act - Second price check still above threshold
        await engine.EvaluateQuoteAsync(quote2);

        // Assert - Email should only be sent ONCE (anti-spam)
        _emailServiceMock.Verify(
            email => email.SendAlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.Equal(StockMonitorEngine.AlertState.SellAlertTriggered, engine.CurrentAlertState);
    }

    [Fact]
    public async Task EvaluateQuoteAsync_WhenPriceBelowBuyTarget_SendsBuyEmailAndUpdatesState()
    {
        // Arrange
        var engine = new StockMonitorEngine(_stockServiceMock.Object, _emailServiceMock.Object, _thresholds);
        var lowQuote = new StockQuote("PETR4", 18.50m, DateTime.UtcNow);

        // Act
        await engine.EvaluateQuoteAsync(lowQuote);

        // Assert
        _emailServiceMock.Verify(
            email => email.SendAlertAsync(
                It.Is<string>(subject => subject.Contains("BUY")),
                It.Is<string>(body => body.Contains("PETR4") && body.Contains("BUYING")),
                It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.Equal(StockMonitorEngine.AlertState.BuyAlertTriggered, engine.CurrentAlertState);
    }

    [Fact]
    public async Task EvaluateQuoteAsync_WhenPriceRemainsBelowBuyTarget_DoesNotSendDuplicateEmail()
    {
        // Arrange
        var engine = new StockMonitorEngine(_stockServiceMock.Object, _emailServiceMock.Object, _thresholds);
        var quote1 = new StockQuote("PETR4", 19.00m, DateTime.UtcNow);
        var quote2 = new StockQuote("PETR4", 18.00m, DateTime.UtcNow);

        // Act
        await engine.EvaluateQuoteAsync(quote1);
        await engine.EvaluateQuoteAsync(quote2);

        // Assert - Only one alert dispatched
        _emailServiceMock.Verify(
            email => email.SendAlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.Equal(StockMonitorEngine.AlertState.BuyAlertTriggered, engine.CurrentAlertState);
    }

    [Fact]
    public async Task EvaluateQuoteAsync_WhenPriceReturnsToNormalRange_ResetsAlertState()
    {
        // Arrange
        var engine = new StockMonitorEngine(_stockServiceMock.Object, _emailServiceMock.Object, _thresholds);
        var sellQuote = new StockQuote("PETR4", 26.00m, DateTime.UtcNow);
        var normalQuote = new StockQuote("PETR4", 22.50m, DateTime.UtcNow);
        var rearmedSellQuote = new StockQuote("PETR4", 26.50m, DateTime.UtcNow);

        // 1. Price breaches sell threshold -> Alert 1 sent
        await engine.EvaluateQuoteAsync(sellQuote);
        Assert.Equal(StockMonitorEngine.AlertState.SellAlertTriggered, engine.CurrentAlertState);

        // 2. Price falls back to normal range -> State resets to None
        await engine.EvaluateQuoteAsync(normalQuote);
        Assert.Equal(StockMonitorEngine.AlertState.None, engine.CurrentAlertState);

        // 3. Price breaches sell threshold again -> Alert 2 sent!
        await engine.EvaluateQuoteAsync(rearmedSellQuote);
        Assert.Equal(StockMonitorEngine.AlertState.SellAlertTriggered, engine.CurrentAlertState);

        // Assert - Total 2 emails sent because of state re-arming
        _emailServiceMock.Verify(
            email => email.SendAlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task EvaluateOnceAsync_WhenQuoteIsNull_DoesNotDispatchEmail()
    {
        // Arrange
        _stockServiceMock
            .Setup(s => s.GetQuoteAsync("PETR4", It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockQuote?)null);

        var engine = new StockMonitorEngine(_stockServiceMock.Object, _emailServiceMock.Object, _thresholds);

        // Act
        await engine.EvaluateOnceAsync();

        // Assert - No email should be sent if quote fetch fails
        _emailServiceMock.Verify(
            email => email.SendAlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);

        Assert.Equal(StockMonitorEngine.AlertState.None, engine.CurrentAlertState);
    }

    [Fact]
    public async Task EvaluateQuoteAsync_WhenQuoteIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var engine = new StockMonitorEngine(_stockServiceMock.Object, _emailServiceMock.Object, _thresholds);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => engine.EvaluateQuoteAsync(null!));
    }
}

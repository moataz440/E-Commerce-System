namespace ECommerceSystem.Patterns;

/// <summary>
/// Strategy pattern for payment processing
/// </summary>
public interface IPaymentStrategy
{
    Task<bool> ProcessPaymentAsync(decimal amount, string paymentDetails);
    Task<bool> RefundAsync(decimal amount, string transactionId);
    string GetPaymentMethodName();
}

public class CreditCardPaymentStrategy : IPaymentStrategy
{
    private readonly ILogger<CreditCardPaymentStrategy> _logger;

    public CreditCardPaymentStrategy(ILogger<CreditCardPaymentStrategy> logger)
    {
        _logger = logger;
    }

    public async Task<bool> ProcessPaymentAsync(decimal amount, string paymentDetails)
    {
        _logger.LogInformation($"Processing credit card payment: ${amount}");
        // Simulate payment processing
        await Task.Delay(500);
        _logger.LogInformation("Credit card payment processed successfully");
        return true;
    }

    public async Task<bool> RefundAsync(decimal amount, string transactionId)
    {
        _logger.LogInformation($"Processing credit card refund: ${amount}, Transaction: {transactionId}");
        await Task.Delay(500);
        return true;
    }

    public string GetPaymentMethodName() => "Credit Card";
}

public class PayPalPaymentStrategy : IPaymentStrategy
{
    private readonly ILogger<PayPalPaymentStrategy> _logger;

    public PayPalPaymentStrategy(ILogger<PayPalPaymentStrategy> logger)
    {
        _logger = logger;
    }

    public async Task<bool> ProcessPaymentAsync(decimal amount, string paymentDetails)
    {
        _logger.LogInformation($"Processing PayPal payment: ${amount}");
        await Task.Delay(800);
        _logger.LogInformation("PayPal payment processed successfully");
        return true;
    }

    public async Task<bool> RefundAsync(decimal amount, string transactionId)
    {
        _logger.LogInformation($"Processing PayPal refund: ${amount}, Transaction: {transactionId}");
        await Task.Delay(800);
        return true;
    }

    public string GetPaymentMethodName() => "PayPal";
}

public class StripePaymentStrategy : IPaymentStrategy
{
    private readonly ILogger<StripePaymentStrategy> _logger;

    public StripePaymentStrategy(ILogger<StripePaymentStrategy> logger)
    {
        _logger = logger;
    }

    public async Task<bool> ProcessPaymentAsync(decimal amount, string paymentDetails)
    {
        _logger.LogInformation($"Processing Stripe payment: ${amount}");
        await Task.Delay(600);
        _logger.LogInformation("Stripe payment processed successfully");
        return true;
    }

    public async Task<bool> RefundAsync(decimal amount, string transactionId)
    {
        _logger.LogInformation($"Processing Stripe refund: ${amount}, Transaction: {transactionId}");
        await Task.Delay(600);
        return true;
    }

    public string GetPaymentMethodName() => "Stripe";
}

/// <summary>
/// Payment processor facade
/// </summary>
public class PaymentProcessor
{
    private IPaymentStrategy _paymentStrategy = null!;
    private readonly ILogger<PaymentProcessor> _logger;

    public PaymentProcessor(ILogger<PaymentProcessor> logger)
    {
        _logger = logger;
    }

    public void SetPaymentStrategy(IPaymentStrategy strategy)
    {
        _paymentStrategy = strategy;
        _logger.LogInformation($"Payment strategy set to: {strategy.GetPaymentMethodName()}");
    }

    public async Task<bool> PayAsync(decimal amount, string paymentDetails)
    {
        if (_paymentStrategy == null)
            throw new InvalidOperationException("Payment strategy not set");

        return await _paymentStrategy.ProcessPaymentAsync(amount, paymentDetails);
    }

    public async Task<bool> RefundAsync(decimal amount, string transactionId)
    {
        if (_paymentStrategy == null)
            throw new InvalidOperationException("Payment strategy not set");

        return await _paymentStrategy.RefundAsync(amount, transactionId);
    }
}

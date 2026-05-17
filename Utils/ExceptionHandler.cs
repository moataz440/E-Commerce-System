using ECommerceSystem.Exceptions;

namespace ECommerceSystem.Utils;

/// <summary>
/// Global exception handling utility
/// </summary>
public class ExceptionHandler
{
    private readonly ILogger<ExceptionHandler> _logger;

    public ExceptionHandler(ILogger<ExceptionHandler> logger)
    {
        _logger = logger;
    }

    public void HandleException(Exception ex)
    {
        switch (ex)
        {
            case EntityNotFoundException enfe:
                _logger.LogWarning($"Entity not found: {enfe.Message}");
                break;

            case ValidationException ve:
                _logger.LogWarning($"Validation error: {ve.Message}");
                break;

            case UnauthorizedAccessException uae:
                _logger.LogWarning($"Unauthorized access: {uae.Message}");
                break;

            case InvalidOperationException ioe:
                _logger.LogWarning($"Invalid operation: {ioe.Message}");
                break;

            case ECommerceException ece:
                _logger.LogError($"E-commerce error [{ece.ErrorCode}]: {ece.Message}");
                break;

            default:
                _logger.LogError(ex, "Unhandled exception occurred");
                break;
        }
    }

    public string GetUserFriendlyMessage(Exception ex)
    {
        return ex switch
        {
            EntityNotFoundException => "The requested item was not found.",
            ValidationException ve => $"Validation error: {ve.Message}",
            UnauthorizedAccessException => "You do not have permission to perform this action.",
            InvalidOperationException ioe => ioe.Message,
            _ => "An unexpected error occurred. Please try again later."
        };
    }
}

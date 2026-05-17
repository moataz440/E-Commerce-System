namespace ECommerceSystem.Exceptions;

public class ECommerceException : Exception
{
    public string? ErrorCode { get; set; }

    public ECommerceException(string message, string? errorCode = null) 
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public ECommerceException(string message, Exception innerException, string? errorCode = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

public class EntityNotFoundException : ECommerceException
{
    public EntityNotFoundException(string entityName, int id)
        : base($"{entityName} with ID {id} not found.", "ENTITY_NOT_FOUND")
    {
    }
}

public class InvalidOperationException : ECommerceException
{
    public InvalidOperationException(string message)
        : base(message, "INVALID_OPERATION")
    {
    }
}

public class ValidationException : ECommerceException
{
    public Dictionary<string, string[]> Errors { get; set; }

    public ValidationException(string message, Dictionary<string, string[]>? errors = null)
        : base(message, "VALIDATION_ERROR")
    {
        Errors = errors ?? new Dictionary<string, string[]>();
    }
}

public class UnauthorizedAccessException : ECommerceException
{
    public UnauthorizedAccessException(string message = "Access denied")
        : base(message, "UNAUTHORIZED")
    {
    }
}

namespace MindReader.Domain.Exceptions;

public class RateLimitException(string message) : Exception(message);

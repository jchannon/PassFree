namespace PassFree;

public enum PassFreeAuthenticaionTokenValidationResult
{
    Success,
    CorrelationMismatch,
    Expired,
    Error
}
namespace PassFree;

public enum PasswordLessAuthenticaionTokenValidationResult
{
    Success,
    CorrelationMismatch,
    Expired,
    Error
}
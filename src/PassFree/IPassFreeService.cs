namespace PassFree;

public interface IPassFreeService
{
    Task<AuthenticationResult> AuthenticateAsync(LoginRequest request);
    Task<RegistrationOptions> GetRegistrationOptionsAsync(string username);
    Task<RegistrationResult> CompleteRegistrationAsync(RegistrationRequest request);
}

public record LoginRequest(string Username, string CredentialId, byte[] Signature);
public record AuthenticationResult(bool Success, string? Token, string? Error);
public record RegistrationOptions(string Challenge, string UserId);
public record RegistrationRequest(string Username, string CredentialId, byte[] PublicKey);
public record RegistrationResult(bool Success, string? Error);

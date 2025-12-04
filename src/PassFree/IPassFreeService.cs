using System.Net.Mail;

namespace PassFree;

public interface IPassFreeService
{
    Task<AuthenticationResult> AuthenticateAsync(LoginRequest request);
    Task<RegistrationOptions> GetRegistrationOptionsAsync(string username);
    Task<RegistrationResult> CompleteRegistrationAsync(RegistrationRequest request);
    
    Task<bool> IsAuthorizedToLogin(MailAddress userAddress) => Task.FromResult(true);
    Task SendLoginEmail(MailAddress userAddress, string loginLink, TimeSpan validFor, CancellationToken none) => Task.CompletedTask; 
}

public record LoginRequest(string Username, string CredentialId, byte[] Signature);
public record AuthenticationResult(bool Success, string? Token, string? Error);
public record RegistrationOptions(string Challenge, string UserId);
public record RegistrationRequest(string Username, string CredentialId, byte[] PublicKey);
public record RegistrationResult(bool Success, string? Error);

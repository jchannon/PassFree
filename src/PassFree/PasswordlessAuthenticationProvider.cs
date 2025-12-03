using System.Net.Mail;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

namespace PassFree;

public class PasswordlessAuthenticationProvider(
    TimeProvider                                clock,
    IDataProtectionProvider                     dataProtectionProvider,
    ILogger<PasswordlessAuthenticationProvider> logger)
{
    private static readonly MailAddress NullUser = new("null@example.com");

    private readonly IDataProtector _dataProtector =
        dataProtectionProvider.CreateProtector("passfree-passwordless-auth");

    public (string AuthToken, string CorrelationToken) GenerateTokens(
        MailAddress user,
        TimeSpan    expiresIn,
        Guid        correlationId)
    {
        var validUntil = clock.GetUtcNow().UtcDateTime.Add(expiresIn);

        var authToken       = new AuthToken(user.Address, validUntil, correlationId);
        var authJson        = JsonSerializer.Serialize(authToken);
        var authTokenString = this._dataProtector.Protect(authJson);

        var correlationToken       = new CorrelationToken(correlationId);
        var correlationJson        = JsonSerializer.Serialize(correlationToken);
        var correlationTokenString = this._dataProtector.Protect(correlationJson);

        return (authTokenString, correlationTokenString);
    }

    /// <summary>
    /// Validates an auth and correlation token.
    /// </summary>
    /// <param name="authTokenString">The auth token as a data protected string.</param>
    /// <param name="correlationTokenString">The correlation token as a data protected string.</param>
    /// <returns>A TokenValidationResult. If successful, also the user's MailAddress.</returns>
    public (PasswordLessAuthenticaionTokenValidationResult ValidatioResult, MailAddress user) ValidateTokens(
        string authTokenString,
        string correlationTokenString)
    {
        try
        {
            var authJson         = this._dataProtector.Unprotect(authTokenString);
            var correlationJson  = this._dataProtector.Unprotect(correlationTokenString);
            var authToken        = JsonSerializer.Deserialize<AuthToken>(authJson)!;
            var correlationToken = JsonSerializer.Deserialize<CorrelationToken>(correlationJson)!;

            // Correlation identifiers must match
            if (authToken.CorrelationId != correlationToken.CorrelationId)
            {
                return (PasswordLessAuthenticaionTokenValidationResult.CorrelationMismatch, NullUser);
            }

            // ValidUntil must greater than current time.
            return authToken.ValidUntil < clock.GetUtcNow().UtcDateTime ?
                (PasswordLessAuthenticaionTokenValidationResult.Expired, NullUser) :
                (PasswordLessAuthenticaionTokenValidationResult.Success, new MailAddress(authToken.EmailAddress));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return (PasswordLessAuthenticaionTokenValidationResult.Error, NullUser);
        }
    }

    private record AuthToken(string EmailAddress, DateTime ValidUntil, Guid CorrelationId);

    private record CorrelationToken(Guid CorrelationId);
}
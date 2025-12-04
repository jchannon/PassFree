using System.Net.Mail;

namespace PassFree;

public interface IPassFreeService
{
    Task<bool> IsAuthorizedToLogin(MailAddress userAddress);

    Task SendLoginEmail(MailAddress userAddress, string loginLink, TimeSpan validFor, CancellationToken none);
}
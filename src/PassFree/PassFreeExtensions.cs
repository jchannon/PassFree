using System.Net.Mail;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PassFree;

public enum LoginStatus
{
    NotAuthenticated,
    RequestEmail,
    LoginLinkSent,
    LoginFailed,
    LoginSucceeded
}

public class PassFreeOptions
{
    public string LoginPath { get; set; } = "/passfree/login";
    public string DefaultRedirectPath { get; set; } = "/login";
    public string SuccessRedirectPath { get; set; } = "/secure";
    public string PageTitle { get; set; } = "PassFree Login";


    public string CorrelationIdCookieKey { get; set; } = "cid";
}

public static class PassFreeExtensions
{
    public static IServiceCollection AddPassFree<T>(this IServiceCollection services,
        Action<PassFreeOptions>? configureOptions = null) where T : class, IPassFreeService
    {
        // Configure options
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.Configure<PassFreeOptions>(_ => { });
        }

        // Register core services
        //services.AddScoped<IPassFreeService, PassFreeService>();
        services.AddSingleton<IPassFreeService, T>();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<PasswordFreeAuthenticationProvider>();

        // Add HttpContextAccessor for navigation
        services.AddHttpContextAccessor();

        // Add tag helpers and razor components support
        services.AddRazorComponents();

        // Add HtmlRenderer for tag helper component rendering
        services.AddScoped<Microsoft.AspNetCore.Components.Web.HtmlRenderer>(sp =>
            new Microsoft.AspNetCore.Components.Web.HtmlRenderer(
                sp,
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>()));

        return services;
    }

    public static IEndpointRouteBuilder MapPassFree(this IEndpointRouteBuilder builder)
    {
        var options = builder.ServiceProvider.GetRequiredService<IOptions<PassFreeOptions>>().Value;

        //builder.MapGet(options.LoginPath, () => "Login page");
        builder.MapPost(options.LoginPath,
            async (HttpContext httpContext, [FromForm] LoginModel model,
                PasswordFreeAuthenticationProvider passwordFreeAuthenticationProvider, ILogger<PassFree> logger,
                IPassFreeService passFreeService) =>
            {
                var status = LoginStatus.NotAuthenticated;
                UriBuilder uriBuilder;
                if (!MailAddress.TryCreate(model.EmailAddress, out var userAddress))
                {
                    uriBuilder = new UriBuilder
                        { Path = options.DefaultRedirectPath, Query = "ErrorMessage=Invalid email address" };
                    return Results.Redirect(uriBuilder.Uri.PathAndQuery);
                }

                var isAuthorizedToLogin = await passFreeService.IsAuthorizedToLogin(userAddress);
                if (!isAuthorizedToLogin)
                {
                    uriBuilder = new UriBuilder
                        { Path = options.DefaultRedirectPath, Query = "Status=LoginFailed" };
                    return Results.Redirect(uriBuilder.Uri.PathAndQuery);
                }

                var validFor = TimeSpan.FromMinutes(5);
                var correlationId = Guid.NewGuid();
                var (authToken, correlationToken) =
                    passwordFreeAuthenticationProvider.GenerateTokens(userAddress, validFor, correlationId);

                var loginLink = GenerateLoginLink(httpContext, authToken, options);
                logger.LogDebug(loginLink);
                await passFreeService.SendLoginEmail(userAddress, loginLink, validFor, CancellationToken.None);

                SetCorrelationIdCookie(httpContext, options, correlationToken);

                uriBuilder = new UriBuilder
                    { Path = options.DefaultRedirectPath, Query = "Status=LoginLinkSent" };
                //httpContext.Response.Redirect(uriBuilder.Uri.PathAndQuery);
                return Results.Redirect(uriBuilder.Uri.PathAndQuery);
            });


        return builder;
    }


    private static string GenerateLoginLink(HttpContext httpContext, string authToken, PassFreeOptions options)
    {
        var request = httpContext.Request;

        var port = request.Host.Port ?? (request.Scheme == "http" ? 80 : 443);

        var loginLinkBuilder = new UriBuilder(request.Scheme, request.Host.Host, port)
        {
            Path = request.PathBase + options.DefaultRedirectPath,
            Query = $"authtoken={authToken}&Status=LoginLinkSent"
        };

        var loginLink = loginLinkBuilder.Uri.ToString();
        return loginLink;
    }

    private static void SetCorrelationIdCookie(HttpContext httpContext, PassFreeOptions options,
        string correlationToken)
    {
        var request = httpContext.Request;
        var response = httpContext.Response;

        var pathBase = request.PathBase;
        response.Cookies.Append(
            options.CorrelationIdCookieKey,
            correlationToken,
            new CookieOptions
            {
                IsEssential = true,
                HttpOnly = true,
                Secure = request.Scheme == "https",
                Path = "/", //pathBase.Value,
                SameSite = SameSiteMode
                    .Lax, // Uses will be clicking a link from mail client, so we'll need this cookie to be Lax.
                Domain = request.Host.Host
            });
    }

    public class LoginModel
    {
        public string EmailAddress { get; set; } = null!;
    }

    public class PassFree
    {
    }
}
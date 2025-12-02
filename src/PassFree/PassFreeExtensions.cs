using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace PassFree;

public class PassFreeOptions
{
    public string LoginPath { get; set; } = "/passfree/login";
    public string DefaultRedirectPath { get; set; } = "/login";
    public string PageTitle { get; set; } = "PassFree Login";
    public string? CustomCssClass { get; set; }
}

public static class PassFreeExtensions
{
    public static IServiceCollection AddPassFree(this IServiceCollection services, Action<PassFreeOptions>? configureOptions = null)
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
        services.AddScoped<IPassFreeService, PassFreeService>();

        // Add tag helpers and razor components support
        services.AddRazorComponents();

        // Add HtmlRenderer for tag helper component rendering
        services.AddSingleton<Microsoft.AspNetCore.Components.Web.HtmlRenderer>(sp =>
            new Microsoft.AspNetCore.Components.Web.HtmlRenderer(
                sp,
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>()));

        return services;
    }

    public static IEndpointRouteBuilder MapPassFree(this IEndpointRouteBuilder builder)
    {
        var options = builder.ServiceProvider.GetRequiredService<IOptions<PassFreeOptions>>().Value;

        //builder.MapGet(options.LoginPath, () => "Login page");
        builder.MapPost(options.LoginPath,  () =>
        {

            return Results.Redirect(options.DefaultRedirectPath + "?ErrorMessage=false");
        });
        

        return builder;
    }
}

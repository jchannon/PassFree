using System.Net.Mail;
using Microsoft.AspNetCore.Authentication.Cookies;
using PassFree;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => { options.LoginPath = "/login"; });

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddPassFree<PassFreeService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();

app.MapPassFree();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

public class PassFreeService : IPassFreeService
{
    public Task<AuthenticationResult> AuthenticateAsync(LoginRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<RegistrationOptions> GetRegistrationOptionsAsync(string username)
    {
        throw new NotImplementedException();
    }

    public Task<RegistrationResult> CompleteRegistrationAsync(RegistrationRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsAuthorizedToLogin(MailAddress userAddress)
    {
        return Task.FromResult(true);
    }

    public Task SendLoginEmail(MailAddress userAddress, string loginLink, TimeSpan validFor, CancellationToken none)
    {
        return Task.CompletedTask;
    }
}
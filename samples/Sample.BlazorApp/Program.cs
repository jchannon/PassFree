using System.Net.Mail;
using Microsoft.AspNetCore.Authentication.Cookies;
using PassFree;
using Sample.BlazorApp.Components;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthorization();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => { options.LoginPath = "/login"; });

builder.Services.AddPassFree<PassFreeService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>();
app.MapPassFree();

app.Run();

public class PassFreeService : IPassFreeService
{
  

    public Task<bool> IsAuthorizedToLogin(MailAddress userAddress)
    {
        //check database if user exists
        return Task.FromResult(true);
    }

    public Task SendLoginEmail(MailAddress userAddress, string loginLink, TimeSpan validFor, CancellationToken none)
    {
        return Task.CompletedTask;
    }
}
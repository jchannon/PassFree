# PassFree

A passwordless authentication library for ASP.NET Core that uses magic links sent via email.

## Features

- 🔐 Secure passwordless authentication using time-limited tokens
- 📧 Magic link authentication via email
- 🎨 Built-in Razor component with customizable UI
- 🔧 Easy integration with ASP.NET Core authentication
- 🛡️ CSRF protection with antiforgery tokens
- 🍪 Secure cookie-based correlation tracking

## How It Works

1. User enters their email address
2. Library generates a secure token and correlation ID
3. Magic link is sent to user's email
4. User clicks the link
5. Library validates the token and correlation cookie
6. User is authenticated and signed in

## Installation

```bash
dotnet add package PassFree
```

## Quick Start

### 1. Implement IPassFreeService

Create a class that implements `IPassFreeService` to customize authorization and email sending:

```csharp
public class MyPassFreeService : IPassFreeService
{
    public async Task<bool> IsAuthorizedToLogin(MailAddress userAddress)
    {
        // Check if user exists in your database
        // Return true to allow login, false to deny
        return await _userRepository.ExistsAsync(userAddress.Address);
    }

    public async Task SendLoginEmail(MailAddress userAddress, string loginLink, TimeSpan validFor, CancellationToken cancellationToken)
    {
        // Send the magic link via your email service
        await _emailService.SendAsync(
            to: userAddress.Address,
            subject: "Login Link",
            body: $"Click here to login: {loginLink} (valid for {validFor.TotalMinutes} minutes)"
        );
    }
}
```

### 2. Configure Services in Blazor Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add authentication
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.LoginPath = "/login";
    });

builder.Services.AddAuthorization();

// Add PassFree with your implementation
builder.Services.AddPassFree<MyPassFreeService>();

// Add Razor Components (for Blazor)
builder.Services.AddRazorComponents();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Map PassFree endpoints (handles POST requests)
app.MapPassFree();

// Map your Razor components
app.MapRazorComponents<App>();

app.Run();
```

### 2. Configure Services in MVC Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => { options.LoginPath = "/login"; });

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddPassFree<PassFreeService>();
var app = builder.Build();

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
```

### 3. Use the PassFreeLogin Component

Create a login page using the built-in component (Blazor):

```razor
@page "/login"
@using PassFree.Components

<PassFreeLogin />
```

Create a login page using the built-in component (MVC View):
```razor
<component type="typeof(PassFree.Components.PassFreeLogin)" render-mode="ServerPrerendered" />
```

That's it! The component handles the entire authentication flow.

## Configuration Options

Customize PassFree behavior with options:

```csharp
builder.Services.AddPassFree<MyPassFreeService>(options =>
{
    options.LoginPath = "/auth/login";              // POST endpoint for login form
    options.DefaultRedirectPath = "/login";          // Where to redirect after email sent
    options.SuccessRedirectPath = "/dashboard";      // Where to redirect after successful login
    options.PageTitle = "Welcome - Sign In";         // Title shown in login component
    options.CorrelationIdCookieKey = "cid";         // Cookie name for correlation ID
});
```

## Styling the Component

The component uses CSS classes that you can style in your own stylesheet:

```css
.passfree-login { }           /* Main container */
.passfree-form-group { }      /* Form group wrapper */
.passfree-error { }           /* Error message */
.login-link-sent { }          /* "Email sent" message */
.login-failed { }             /* Failed login message */
.login-succeeded { }          /* Success message */
```

## Security Features

- **Time-limited tokens**: Login links expire after 5 minutes
- **Correlation cookies**: Prevents CSRF attacks by requiring the link to be clicked from the same browser
- **Secure cookies**: HttpOnly, Secure (HTTPS), SameSite=Lax
- **Token validation**: Both auth token and correlation token must match
- **Authorization hooks**: Check user authorization before sending login links

## License

MIT

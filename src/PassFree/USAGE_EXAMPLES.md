# PassFree Usage Examples

## Setup (Both MVC and Blazor)

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Configure PassFree
builder.Services.AddPassFree(options =>
{
    options.LoginPath = "/auth/login";
    options.PageTitle = "Welcome - Sign In";
    options.CustomCssClass = "my-custom-theme";
});

var app = builder.Build();

// Map PassFree endpoints (provides API endpoints for authentication)
app.MapPassFree();

app.Run();
```

---

## For MVC / Razor Pages Users

### 1. Enable Tag Helpers

Create or update `_ViewImports.cshtml`:

```cshtml
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, PassFree
```

### 2. Use the Tag Helper in Your View

```cshtml
@* Pages/Login.cshtml or Views/Account/Login.cshtml *@

<!DOCTYPE html>
<html>
<head>
    <title>Login</title>
</head>
<body>
    <passfree-login
        title="Sign In to Your Account"
        action-url="/auth/login"
        css-class="custom-login-style">
    </passfree-login>
</body>
</html>
```

### 3. Custom Controller Using the Service

```csharp
public class AuthController : Controller
{
    private readonly IPassFreeService _passFree;

    public AuthController(IPassFreeService passFree)
    {
        _passFree = passFree;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _passFree.AuthenticateAsync(request);

        if (result.Success)
        {
            // Set authentication cookie, redirect, etc.
            return Ok(new { token = result.Token });
        }

        return BadRequest(new { error = result.Error });
    }
}
```

---

## For Blazor Users

### 1. Configure Blazor in Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorPages();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add PassFree
builder.Services.AddPassFree(options =>
{
    options.LoginPath = "/api/passfree/login";
    options.PageTitle = "Secure Login";
});

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPassFree();

app.Run();
```

### 2. Use the Razor Component

```razor
@* Pages/Login.razor *@
@page "/login"
@using PassFree.Components

<PageTitle>Login</PageTitle>

<h1>Sign In</h1>

<PassFreeLogin
    Title="Welcome Back"
    ActionUrl="/api/passfree/login"
    CssClass="custom-style"
    OnSuccess="HandleLoginSuccess" />

@code {
    private async Task HandleLoginSuccess(string action)
    {
        // Handle successful authentication
        Console.WriteLine($"Action: {action}");
        // Navigate to home page, etc.
    }
}
```

### 3. Custom Blazor Component Using the Service

```razor
@page "/custom-login"
@inject IPassFreeService PassFree

<h1>Custom Login</h1>

<EditForm Model="loginModel" OnValidSubmit="HandleLogin">
    <InputText @bind-Value="loginModel.Username" />
    <button type="submit">Login</button>
</EditForm>

@if (!string.IsNullOrEmpty(errorMessage))
{
    <div class="alert alert-danger">@errorMessage</div>
}

@code {
    private LoginModel loginModel = new();
    private string? errorMessage;

    private async Task HandleLogin()
    {
        var request = new LoginRequest(
            loginModel.Username,
            "credential-id",
            Array.Empty<byte>());

        var result = await PassFree.AuthenticateAsync(request);

        if (result.Success)
        {
            // Navigate to home
        }
        else
        {
            errorMessage = result.Error;
        }
    }

    class LoginModel
    {
        public string Username { get; set; } = "";
    }
}
```

---

## Key Points

### ✅ Both MVC and Blazor can:
- Use the default UI (Tag Helper for MVC, Razor Component for Blazor)
- Inject `IPassFreeService` to build completely custom UI
- Configure options via `AddPassFree()`
- Call the same API endpoints via `MapPassFree()`

### 🎨 Customization Options:
1. **Use defaults** - Just call `AddPassFree()` and use the component/tag helper
2. **Configure options** - Customize paths, titles, CSS classes
3. **Custom UI with service** - Build your own UI, inject `IPassFreeService`
4. **Override styles** - Add your own CSS to style the default components

### 🔒 Authentication Logic:
- **Always in the library** via `IPassFreeService`
- UI is just a presentation layer
- Consumers can replace UI but keep your authentication logic

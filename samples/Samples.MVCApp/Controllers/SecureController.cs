using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Samples.MVCApp.Controllers;

[Authorize]
public class SecureController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}
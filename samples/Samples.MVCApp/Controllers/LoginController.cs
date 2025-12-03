using Microsoft.AspNetCore.Mvc;

namespace Samples.MVCApp.Controllers;

public class LoginController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}
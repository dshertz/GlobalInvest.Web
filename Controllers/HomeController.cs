using Microsoft.AspNetCore.Mvc;

public partial class HomeController : Controller
{
    public HomeController() { }

    public IActionResult Index()
    {
        return View();
    }
}

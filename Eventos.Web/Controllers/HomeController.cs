using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Eventos.Web.Models;

namespace Eventos.Web.Controllers;

public class HomeController : Controller
{
    // La página de inicio es el listado de eventos
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Eventos");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

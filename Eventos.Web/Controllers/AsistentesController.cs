using Eventos.Web.Models;
using Eventos.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;

namespace Eventos.Web.Controllers;

public class AsistentesController : Controller
{
    private readonly AsistenteRepositorio _asistentes;

    public AsistentesController(AsistenteRepositorio asistentes)
    {
        _asistentes = asistentes;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _asistentes.ListarActivosAsync());
    }

    public IActionResult Create()
    {
        return View(new Asistente());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nombres,Dni,Correo,Telefono")] Asistente asistente)
    {
        asistente.Correo = asistente.Correo?.Trim().ToLower() ?? string.Empty;

        // Se revisa antes de guardar para mostrar el error en el campo y no una excepción del índice único
        if (!string.IsNullOrEmpty(asistente.Dni) && await _asistentes.ExisteDniAsync(asistente.Dni))
            ModelState.AddModelError(nameof(Asistente.Dni), "Ya existe un asistente con ese DNI.");
        if (!string.IsNullOrEmpty(asistente.Correo) && await _asistentes.ExisteCorreoAsync(asistente.Correo))
            ModelState.AddModelError(nameof(Asistente.Correo), "Ya existe un asistente con ese correo.");

        if (!ModelState.IsValid)
            return View(asistente);

        await _asistentes.CrearAsync(asistente);
        TempData["Mensaje"] = $"Asistente {asistente.Nombres} registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }
}

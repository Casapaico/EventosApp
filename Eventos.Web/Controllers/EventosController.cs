using Eventos.Web.Models;
using Eventos.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Eventos.Web.Controllers;

public class EventosController : Controller
{
    private readonly EventoRepositorio _eventos;

    public EventosController(EventoRepositorio eventos)
    {
        _eventos = eventos;
    }

    // Filtro por GET: /Eventos?categoriaId=2
    public async Task<IActionResult> Index(int? categoriaId)
    {
        var categorias = await _eventos.ObtenerCategoriasActivasAsync();
        ViewBag.Categorias = new SelectList(categorias, nameof(Categoria.Id), nameof(Categoria.Nombre), categoriaId);
        return View(await _eventos.ListarActivosAsync(categoriaId));
    }

    public async Task<IActionResult> Details(int id)
    {
        var evento = await _eventos.ObtenerActivoAsync(id);
        if (evento == null)
            return NotFound();

        ViewBag.Inscritos = await _eventos.ListarInscritosAsync(id);
        return View(evento);
    }

    public async Task<IActionResult> Create()
    {
        // Valores por defecto para que el formulario no arranque en 01/01/0001
        var evento = new Evento { Fecha = DateTime.Today.AddDays(7).AddHours(18), Aforo = 50 };
        await CargarCategoriasAsync(evento.CategoriaId);
        return View(evento);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Titulo,Fecha,Lugar,Aforo,Precio,CategoriaId")] Evento evento)
    {
        await ValidarCategoriaAsync(evento);

        if (!ModelState.IsValid)
        {
            await CargarCategoriasAsync(evento.CategoriaId);
            return View(evento);
        }

        await _eventos.CrearAsync(evento);
        TempData["Mensaje"] = $"Evento \"{evento.Titulo}\" registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var evento = await _eventos.ObtenerActivoAsync(id);
        if (evento == null)
            return NotFound();

        await CargarCategoriasAsync(evento.CategoriaId);
        return View(evento);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Titulo,Fecha,Lugar,Aforo,Precio,CategoriaId")] Evento evento)
    {
        if (id != evento.Id)
            return NotFound();

        await ValidarCategoriaAsync(evento);

        // No se puede bajar el aforo por debajo de los que ya están inscritos
        var confirmados = await _eventos.ContarConfirmadosAsync(id);
        if (evento.Aforo < confirmados)
            ModelState.AddModelError(nameof(Evento.Aforo), $"El evento ya tiene {confirmados} inscritos confirmados.");

        if (!ModelState.IsValid)
        {
            await CargarCategoriasAsync(evento.CategoriaId);
            return View(evento);
        }

        if (!await _eventos.ActualizarAsync(evento))
            return NotFound();

        TempData["Mensaje"] = $"Evento \"{evento.Titulo}\" actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var evento = await _eventos.ObtenerActivoAsync(id);
        if (evento == null)
            return NotFound();

        ViewBag.Confirmados = await _eventos.ContarConfirmadosAsync(id);
        return View(evento);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (await _eventos.EliminarAsync(id))
            TempData["Mensaje"] = "Evento eliminado correctamente.";
        else
            TempData["Error"] = "El evento no existe o ya fue eliminado.";

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidarCategoriaAsync(Evento evento)
    {
        // La navegación no viene del formulario
        ModelState.Remove(nameof(Evento.Categoria));

        if (evento.CategoriaId > 0 && !await _eventos.ExisteCategoriaActivaAsync(evento.CategoriaId))
            ModelState.AddModelError(nameof(Evento.CategoriaId), "La categoría seleccionada no existe o está inactiva.");
    }

    private async Task CargarCategoriasAsync(int categoriaId)
    {
        var categorias = await _eventos.ObtenerCategoriasActivasAsync();
        ViewBag.Categorias = new SelectList(categorias, nameof(Categoria.Id), nameof(Categoria.Nombre), categoriaId);
    }
}

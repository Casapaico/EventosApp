using Eventos.Web.Models;
using Eventos.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Eventos.Web.Controllers;

public class InscripcionesController : Controller
{
    private readonly InscripcionRepositorio _inscripciones;

    public InscripcionesController(InscripcionRepositorio inscripciones)
    {
        _inscripciones = inscripciones;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _inscripciones.ListarAsync());
    }

    // Puede llegar con el evento ya elegido desde el listado (botón "Inscribir")
    public async Task<IActionResult> Create(int? eventoId)
    {
        var inscripcion = new Inscripcion { EventoId = eventoId ?? 0 };
        await CargarListasAsync(inscripcion);
        return View(inscripcion);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("EventoId,AsistenteId")] Inscripcion inscripcion)
    {
        ModelState.Remove(nameof(Inscripcion.Evento));
        ModelState.Remove(nameof(Inscripcion.Asistente));
        await ValidarReglasAsync(inscripcion);

        if (!ModelState.IsValid)
        {
            await CargarListasAsync(inscripcion);
            return View(inscripcion);
        }

        await _inscripciones.CrearAsync(inscripcion);
        TempData["Mensaje"] = $"Inscripción N.° {inscripcion.Id} registrada como Confirmada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        if (await _inscripciones.CancelarAsync(id))
            TempData["Mensaje"] = $"Inscripción N.° {id} cancelada correctamente.";
        else
            TempData["Error"] = $"La inscripción N.° {id} no existe o ya estaba cancelada.";

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidarReglasAsync(Inscripcion inscripcion)
    {
        if (inscripcion.AsistenteId > 0 && !await _inscripciones.ExisteAsistenteActivoAsync(inscripcion.AsistenteId))
            ModelState.AddModelError(nameof(Inscripcion.AsistenteId), "El asistente seleccionado no existe o está inactivo.");

        if (inscripcion.EventoId <= 0)
            return;

        var evento = await _inscripciones.ObtenerEventoActivoAsync(inscripcion.EventoId);
        if (evento == null)
        {
            ModelState.AddModelError(nameof(Inscripcion.EventoId), "El evento seleccionado no existe o fue eliminado.");
            return;
        }

        // Regla 1: el evento no debe haber pasado
        if (evento.Fecha < DateTime.Now)
        {
            ModelState.AddModelError(nameof(Inscripcion.EventoId),
                $"El evento \"{evento.Titulo}\" ya se realizó el {evento.Fecha:dd/MM/yyyy}.");
            return;
        }

        // Regla 2: el asistente no debe estar ya inscrito (Confirmada) en ese evento
        if (inscripcion.AsistenteId > 0 && await _inscripciones.YaInscritoAsync(evento.Id, inscripcion.AsistenteId))
            ModelState.AddModelError(nameof(Inscripcion.AsistenteId), "El asistente ya está inscrito en este evento.");

        // Regla 3: debe quedar cupo (confirmadas < aforo)
        var confirmadas = await _inscripciones.ContarConfirmadasAsync(evento.Id);
        if (confirmadas >= evento.Aforo)
            ModelState.AddModelError(nameof(Inscripcion.EventoId),
                $"El aforo está completo ({confirmadas} de {evento.Aforo} inscritos).");
    }

    private async Task CargarListasAsync(Inscripcion inscripcion)
    {
        var eventos = await _inscripciones.ObtenerEventosActivosAsync();
        ViewBag.Eventos = eventos.Select(e => new SelectListItem(
            $"{e.Titulo} - {e.Fecha:dd/MM/yyyy HH:mm}",
            e.Id.ToString(),
            e.Id == inscripcion.EventoId));

        var asistentes = await _inscripciones.ObtenerAsistentesActivosAsync();
        ViewBag.Asistentes = asistentes.Select(a => new SelectListItem(
            $"{a.Nombres} - DNI {a.Dni}",
            a.Id.ToString(),
            a.Id == inscripcion.AsistenteId));
    }
}

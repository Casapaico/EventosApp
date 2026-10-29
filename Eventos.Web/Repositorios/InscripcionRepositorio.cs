using Eventos.Web.Data;
using Eventos.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Eventos.Web.Repositorios;

public class InscripcionRepositorio
{
    private readonly EventosContext _context;

    public InscripcionRepositorio(EventosContext context)
    {
        _context = context;
    }

    // Inscripciones con evento y asistente, las más recientes primero
    public async Task<List<Inscripcion>> ListarAsync()
    {
        return await _context.Inscripciones
            .AsNoTracking()
            .Include(i => i.Evento)
            .Include(i => i.Asistente)
            .OrderByDescending(i => i.FechaInscripcion)
            .ToListAsync();
    }

    // Datos de las listas desplegables (solo activos)
    public async Task<List<Evento>> ObtenerEventosActivosAsync()
    {
        return await _context.Eventos
            .AsNoTracking()
            .Where(e => e.Activo)
            .OrderBy(e => e.Fecha)
            .ToListAsync();
    }

    public async Task<List<Asistente>> ObtenerAsistentesActivosAsync()
    {
        return await _context.Asistentes
            .AsNoTracking()
            .Where(a => a.Activo)
            .OrderBy(a => a.Nombres)
            .ToListAsync();
    }

    public async Task<Evento?> ObtenerEventoActivoAsync(int eventoId)
    {
        return await _context.Eventos
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == eventoId && e.Activo);
    }

    public async Task<bool> ExisteAsistenteActivoAsync(int asistenteId)
    {
        return await _context.Asistentes.AnyAsync(a => a.Id == asistenteId && a.Activo);
    }

    // Una inscripción cancelada no cuenta: el asistente puede volver a inscribirse
    public async Task<bool> YaInscritoAsync(int eventoId, int asistenteId)
    {
        return await _context.Inscripciones.AnyAsync(i =>
            i.EventoId == eventoId && i.AsistenteId == asistenteId && i.Estado == "Confirmada");
    }

    public async Task<int> ContarConfirmadasAsync(int eventoId)
    {
        return await _context.Inscripciones
            .CountAsync(i => i.EventoId == eventoId && i.Estado == "Confirmada");
    }

    public async Task CrearAsync(Inscripcion inscripcion)
    {
        inscripcion.Estado = "Confirmada";
        inscripcion.FechaInscripcion = DateTime.Now;
        _context.Inscripciones.Add(inscripcion);
        await _context.SaveChangesAsync();
    }

    // Cancelar no borra la fila, solo cambia el estado
    public async Task<bool> CancelarAsync(int id)
    {
        var inscripcion = await _context.Inscripciones.FindAsync(id);
        if (inscripcion == null || inscripcion.Estado != "Confirmada")
            return false;

        inscripcion.Estado = "Cancelada";
        await _context.SaveChangesAsync();
        return true;
    }
}

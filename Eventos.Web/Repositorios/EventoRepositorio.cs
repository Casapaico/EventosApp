using Eventos.Web.Data;
using Eventos.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Eventos.Web.Repositorios;

public class EventoRepositorio
{
    private readonly EventosContext _context;

    public EventoRepositorio(EventosContext context)
    {
        _context = context;
    }

    // Eventos activos con su categoría y los inscritos confirmados, filtro opcional por categoría.
    // El Count dentro del Select se traduce a una subconsulta, no trae las inscripciones a memoria.
    public async Task<List<EventoListadoItem>> ListarActivosAsync(int? categoriaId)
    {
        var consulta = _context.Eventos
            .AsNoTracking()
            .Where(e => e.Activo);

        if (categoriaId.HasValue)
            consulta = consulta.Where(e => e.CategoriaId == categoriaId);

        return await consulta
            .OrderBy(e => e.Fecha)
            .Select(e => new EventoListadoItem
            {
                Id = e.Id,
                Titulo = e.Titulo,
                Fecha = e.Fecha,
                Lugar = e.Lugar,
                Modalidad = e.Modalidad,
                Categoria = e.Categoria!.Nombre,
                Aforo = e.Aforo,
                Precio = e.Precio,
                Confirmados = e.Inscripciones.Count(i => i.Estado == "Confirmada")
            })
            .ToListAsync();
    }

    public async Task<Evento?> ObtenerActivoAsync(int id)
    {
        return await _context.Eventos
            .AsNoTracking()
            .Include(e => e.Categoria)
            .FirstOrDefaultAsync(e => e.Id == id && e.Activo);
    }

    // Asistentes con inscripción confirmada en el evento (para la vista Details)
    public async Task<List<Asistente>> ListarInscritosAsync(int eventoId)
    {
        return await _context.Inscripciones
            .AsNoTracking()
            .Where(i => i.EventoId == eventoId && i.Estado == "Confirmada")
            .OrderBy(i => i.Asistente!.Nombres)
            .Select(i => i.Asistente!)
            .ToListAsync();
    }

    public async Task<int> ContarConfirmadosAsync(int eventoId)
    {
        return await _context.Inscripciones
            .CountAsync(i => i.EventoId == eventoId && i.Estado == "Confirmada");
    }

    // Datos de la lista desplegable (solo categorías activas)
    public async Task<List<Categoria>> ObtenerCategoriasActivasAsync()
    {
        return await _context.Categorias
            .AsNoTracking()
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task<bool> ExisteCategoriaActivaAsync(int categoriaId)
    {
        return await _context.Categorias.AnyAsync(c => c.Id == categoriaId && c.Activo);
    }

    public async Task CrearAsync(Evento evento)
    {
        evento.Activo = true;
        _context.Eventos.Add(evento);
        await _context.SaveChangesAsync();
    }

    // Se copian solo los campos del formulario; Activo no se toca desde Edit
    public async Task<bool> ActualizarAsync(Evento datos)
    {
        var evento = await _context.Eventos.FirstOrDefaultAsync(e => e.Id == datos.Id && e.Activo);
        if (evento == null)
            return false;

        evento.Titulo = datos.Titulo;
        evento.Fecha = datos.Fecha;
        evento.Lugar = datos.Lugar;
        evento.Modalidad = datos.Modalidad;
        evento.Aforo = datos.Aforo;
        evento.Precio = datos.Precio;
        evento.CategoriaId = datos.CategoriaId;
        await _context.SaveChangesAsync();
        return true;
    }

    // Eliminación lógica: el registro queda en la tabla con Activo = false
    public async Task<bool> EliminarAsync(int id)
    {
        var evento = await _context.Eventos.FirstOrDefaultAsync(e => e.Id == id && e.Activo);
        if (evento == null)
            return false;

        evento.Activo = false;
        await _context.SaveChangesAsync();
        return true;
    }
}

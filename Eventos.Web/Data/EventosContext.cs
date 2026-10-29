using Eventos.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Eventos.Web.Data;

// Code First: las tablas salen de estas clases y de sus Data Annotations (sin Fluent API)
public class EventosContext : DbContext
{
    public EventosContext(DbContextOptions<EventosContext> options) : base(options)
    {
    }

    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Evento> Eventos { get; set; }
    public DbSet<Asistente> Asistentes { get; set; }
    public DbSet<Inscripcion> Inscripciones { get; set; }
}

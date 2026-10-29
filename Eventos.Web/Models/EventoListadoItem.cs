namespace Eventos.Web.Models;

// Fila del listado de eventos: datos del evento + conteo de inscritos confirmados
public class EventoListadoItem
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string Lugar { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public int Aforo { get; set; }
    public decimal Precio { get; set; }
    public int Confirmados { get; set; }

    public int CuposDisponibles => Aforo - Confirmados;
}

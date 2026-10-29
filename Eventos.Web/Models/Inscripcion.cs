using System.ComponentModel.DataAnnotations;

namespace Eventos.Web.Models;

public class Inscripcion
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un evento.")]
    [Display(Name = "Evento")]
    public int EventoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un asistente.")]
    [Display(Name = "Asistente")]
    public int AsistenteId { get; set; }

    [Display(Name = "Fecha de inscripción")]
    public DateTime FechaInscripcion { get; set; }

    // Solo dos valores posibles: Confirmada o Cancelada
    [Required]
    [StringLength(20)]
    [AllowedValues("Confirmada", "Cancelada")]
    public string Estado { get; set; } = "Confirmada";

    public Evento? Evento { get; set; }

    public Asistente? Asistente { get; set; }
}

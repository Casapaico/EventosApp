using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eventos.Web.Models;

public class Evento
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(100, ErrorMessage = "El título no puede superar los 100 caracteres.")]
    [Display(Name = "Título")]
    public string Titulo { get; set; } = string.Empty;

    [DataType(DataType.DateTime)]
    public DateTime Fecha { get; set; }

    [Required(ErrorMessage = "El lugar es obligatorio.")]
    [StringLength(100, ErrorMessage = "El lugar no puede superar los 100 caracteres.")]
    public string Lugar { get; set; } = string.Empty;

    [Range(1, 5000, ErrorMessage = "El aforo debe estar entre 1 y 5000.")]
    public int Aforo { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    [Range(typeof(decimal), "0", "99999999.99", ErrorMessage = "El precio no puede ser negativo.")]
    public decimal Precio { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una categoría.")]
    [Display(Name = "Categoría")]
    public int CategoriaId { get; set; }

    // Agregada en la migración AgregarModalidadEvento
    [Required(ErrorMessage = "La modalidad es obligatoria.")]
    [StringLength(20)]
    [AllowedValues("Presencial", "Virtual", ErrorMessage = "La modalidad debe ser Presencial o Virtual.")]
    public string Modalidad { get; set; } = "Presencial";

    public bool Activo { get; set; } = true;

    public Categoria? Categoria { get; set; }

    public List<Inscripcion> Inscripciones { get; set; } = new();
}

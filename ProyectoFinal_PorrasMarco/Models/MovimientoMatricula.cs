using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal_PorrasMarco.Models;

public class MovimientoMatricula
{
    public int Id { get; set; }

    public int CursoId { get; set; }

    public Curso Curso { get; set; } = null!;

    public int EstudianteId { get; set; }

    public Estudiante Estudiante { get; set; } = null!;

    [MaxLength(450)]
    public string UsuarioResponsableId { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Accion { get; set; } = string.Empty;

    [MaxLength(20)]
    public string EstadoAnterior { get; set; } = string.Empty;

    [MaxLength(20)]
    public string EstadoNuevo { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Justificacion { get; set; } = string.Empty;

    public DateTime Fecha { get; set; }
}

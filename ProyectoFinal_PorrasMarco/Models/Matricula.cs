using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal_PorrasMarco.Models;

public class Matricula
{
    public int Id { get; set; }

    public int EstudianteId { get; set; }

    public Estudiante Estudiante { get; set; } = null!;

    public int CursoId { get; set; }

    public Curso Curso { get; set; } = null!;

    [MaxLength(40)]
    public string Periodo { get; set; } = string.Empty;

    public EstadoMatricula Estado { get; set; } = EstadoMatricula.Activa;

    public DateTime FechaMatricula { get; set; }

    public DateTime? FechaRetiro { get; set; }

    public Calificacion? Calificacion { get; set; }
}

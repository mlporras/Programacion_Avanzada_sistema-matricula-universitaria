using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal_PorrasMarco.Models;

public class Carrera
{
    public int Id { get; set; }

    [MaxLength(20)]
    public string Codigo { get; set; } = string.Empty;

    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    public bool Activa { get; set; } = true;

    public bool EsDatoDemostracion { get; set; }

    public ICollection<Curso> Cursos { get; set; } = new List<Curso>();

    public ICollection<Estudiante> Estudiantes { get; set; } = new List<Estudiante>();
}

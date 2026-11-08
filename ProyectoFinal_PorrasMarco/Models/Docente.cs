using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal_PorrasMarco.Models;

public class Docente
{
    public int Id { get; set; }

    [MaxLength(450)]
    public string UsuarioId { get; set; } = string.Empty;

    public ApplicationUser Usuario { get; set; } = null!;

    [MaxLength(120)]
    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public bool EsDatoDemostracion { get; set; }

    public ICollection<Curso> Cursos { get; set; } = new List<Curso>();
}

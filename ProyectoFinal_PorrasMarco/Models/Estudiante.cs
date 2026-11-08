using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal_PorrasMarco.Models;

public class Estudiante
{
    public int Id { get; set; }

    [MaxLength(450)]
    public string UsuarioId { get; set; } = string.Empty;

    public ApplicationUser Usuario { get; set; } = null!;

    [MaxLength(120)]
    public string Nombre { get; set; } = string.Empty;

    public int? CarreraId { get; set; }

    public Carrera? Carrera { get; set; }

    public bool Activo { get; set; } = true;

    public bool EsDatoDemostracion { get; set; }

    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
}

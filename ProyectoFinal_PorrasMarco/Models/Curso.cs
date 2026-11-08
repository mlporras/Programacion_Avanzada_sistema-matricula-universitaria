using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal_PorrasMarco.Models;

public class Curso
{
    public int Id { get; set; }

    [MaxLength(20)]
    public string Codigo { get; set; } = string.Empty;

    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    public int? Creditos { get; set; }

    public int CarreraId { get; set; }

    public Carrera Carrera { get; set; } = null!;

    [MaxLength(40)]
    public string Periodo { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public int? DocenteId { get; set; }

    public Docente? Docente { get; set; }

    public bool EsDatoDemostracion { get; set; }

    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
}

using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal_PorrasMarco.Models;

public class Calificacion
{
    public int Id { get; set; }

    public int MatriculaId { get; set; }

    public Matricula Matricula { get; set; } = null!;

    public decimal? Valor { get; set; }

    public bool EsDatoDemostracion { get; set; }

    public DateTime? FechaRegistro { get; set; }

    [MaxLength(80)]
    public string EscalaReferencia { get; set; } = DatosDemostracion.Escala;
}

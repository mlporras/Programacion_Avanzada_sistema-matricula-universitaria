using Microsoft.EntityFrameworkCore;
using ProyectoFinal_PorrasMarco.Data;
using ProyectoFinal_PorrasMarco.Models;
using ProyectoFinal_PorrasMarco.ViewModels;

namespace ProyectoFinal_PorrasMarco.Servicios;

public class ReporteServicio
{
    private readonly ApplicationDbContext _db;

    public ReporteServicio(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ReportePersonalVm?> ConsultarPropioAsync(string usuarioId, string? periodo)
    {
        var estudiante = await _db.Estudiantes.AsNoTracking()
            .FirstOrDefaultAsync(item => item.UsuarioId == usuarioId);
        if (estudiante is null)
        {
            return null;
        }

        var matriculas = await _db.Matriculas.AsNoTracking()
            .Include(matricula => matricula.Curso)
            .Include(matricula => matricula.Calificacion)
            .Where(matricula => matricula.EstudianteId == estudiante.Id)
            .ToListAsync();

        var periodosCatalogo = await _db.Cursos.AsNoTracking()
            .Select(curso => curso.Periodo)
            .Distinct()
            .ToListAsync();
        var periodos = matriculas.Select(matricula => matricula.Periodo)
            .Concat(periodosCatalogo)
            .Distinct()
            .OrderBy(item => item)
            .ToList();

        var seleccionado = string.IsNullOrWhiteSpace(periodo)
            ? periodos.FirstOrDefault() ?? string.Empty
            : periodo.Trim();

        var delPeriodo = matriculas.Where(matricula => matricula.Periodo == seleccionado).ToList();
        var hayNotas = delPeriodo.Any(matricula => matricula.Calificacion?.Valor is not null);

        return new ReportePersonalVm
        {
            Periodo = seleccionado,
            Periodos = periodos,
            Vacio = !hayNotas,
            Escala = DatosDemostracion.Escala,
            Filas = hayNotas
                ? delPeriodo.OrderBy(matricula => matricula.Curso.Codigo).Select(matricula => new FilaReporteVm
                {
                    Curso = $"{matricula.Curso.Codigo} · {matricula.Curso.Nombre}",
                    Valor = matricula.Calificacion?.Valor,
                    EsDemostracion = matricula.Calificacion?.EsDatoDemostracion == true,
                    Estado = matricula.Estado == EstadoMatricula.Activa ? "Activa" : "Retirada"
                }).ToList()
                : []
        };
    }
}

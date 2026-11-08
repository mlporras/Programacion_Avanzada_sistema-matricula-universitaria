using Microsoft.EntityFrameworkCore;
using ProyectoFinal_PorrasMarco.Data;
using ProyectoFinal_PorrasMarco.Models;

namespace ProyectoFinal_PorrasMarco.Servicios;

public class ListaCursoServicio
{
    private readonly ApplicationDbContext _db;

    public ListaCursoServicio(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ResultadoOperacion> AgregarAsync(string usuarioDocenteId, int cursoId, int estudianteId, string? justificacion)
    {
        var texto = justificacion?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(texto))
        {
            return ResultadoOperacion.Fallo("La justificación es obligatoria.", "Justificacion");
        }

        var curso = await CursoAsignadoAsync(usuarioDocenteId, cursoId);
        if (curso is null)
        {
            return ResultadoOperacion.Fallo("Acceso no autorizado.");
        }

        var estudiante = await _db.Estudiantes.FirstOrDefaultAsync(item => item.Id == estudianteId);
        if (estudiante is null || !estudiante.Activo || estudiante.CarreraId != curso.CarreraId)
        {
            return ResultadoOperacion.Fallo("El estudiante no está activo en la carrera del curso. La lista no cambió.");
        }

        var yaActiva = await _db.Matriculas.AnyAsync(matricula =>
            matricula.EstudianteId == estudiante.Id &&
            matricula.CursoId == curso.Id &&
            matricula.Estado == EstadoMatricula.Activa);
        if (yaActiva)
        {
            return ResultadoOperacion.Fallo("El estudiante ya está activo en el curso. La lista no cambió.");
        }

        await using var transaccion = await _db.Database.BeginTransactionAsync();
        var matricula = new Matricula
        {
            EstudianteId = estudiante.Id,
            CursoId = curso.Id,
            Periodo = curso.Periodo,
            Estado = EstadoMatricula.Activa,
            FechaMatricula = DateTime.Now
        };
        _db.Matriculas.Add(matricula);
        _db.MovimientosMatricula.Add(new MovimientoMatricula
        {
            CursoId = curso.Id,
            EstudianteId = estudiante.Id,
            UsuarioResponsableId = usuarioDocenteId,
            Accion = "Agregar",
            EstadoAnterior = "Sin matrícula activa",
            EstadoNuevo = EstadoMatricula.Activa.ToString(),
            Justificacion = texto,
            Fecha = DateTime.Now
        });

        try
        {
            await _db.SaveChangesAsync();
            await transaccion.CommitAsync();
        }
        catch (DbUpdateException)
        {
            return ResultadoOperacion.Fallo("El estudiante ya está activo en el curso. La lista no cambió.");
        }

        return ResultadoOperacion.Ok("Estudiante agregado", matricula.Id);
    }

    public async Task<ResultadoOperacion> RetirarAsync(string usuarioDocenteId, int cursoId, int matriculaId, string? justificacion)
    {
        var texto = justificacion?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(texto))
        {
            return ResultadoOperacion.Fallo("La justificación es obligatoria.", "Justificacion");
        }

        var curso = await CursoAsignadoAsync(usuarioDocenteId, cursoId);
        if (curso is null)
        {
            return ResultadoOperacion.Fallo("Acceso no autorizado.");
        }

        var matricula = await _db.Matriculas
            .Include(item => item.Calificacion)
            .FirstOrDefaultAsync(item => item.Id == matriculaId && item.CursoId == curso.Id);
        if (matricula is null || matricula.Estado != EstadoMatricula.Activa)
        {
            return ResultadoOperacion.Fallo("El estudiante no tiene una matrícula activa en el curso. La lista no cambió.");
        }

        var nota = matricula.Calificacion?.Valor;
        await using var transaccion = await _db.Database.BeginTransactionAsync();
        matricula.Estado = EstadoMatricula.Retirada;
        matricula.FechaRetiro = DateTime.Now;
        _db.MovimientosMatricula.Add(new MovimientoMatricula
        {
            CursoId = curso.Id,
            EstudianteId = matricula.EstudianteId,
            UsuarioResponsableId = usuarioDocenteId,
            Accion = "Retirar",
            EstadoAnterior = EstadoMatricula.Activa.ToString(),
            EstadoNuevo = EstadoMatricula.Retirada.ToString(),
            Justificacion = texto,
            Fecha = matricula.FechaRetiro.Value
        });
        await _db.SaveChangesAsync();
        await transaccion.CommitAsync();
        return ResultadoOperacion.Ok("Estudiante removido", matricula.Id, nota?.ToString());
    }

    private Task<Curso?> CursoAsignadoAsync(string usuarioDocenteId, int cursoId) =>
        _db.Cursos.Include(curso => curso.Docente)
            .FirstOrDefaultAsync(curso =>
                curso.Id == cursoId &&
                curso.Activo &&
                curso.Docente != null &&
                curso.Docente.Activo &&
                curso.Docente.UsuarioId == usuarioDocenteId);
}

using Microsoft.EntityFrameworkCore;
using ProyectoFinal_PorrasMarco.Data;
using ProyectoFinal_PorrasMarco.Models;

namespace ProyectoFinal_PorrasMarco.Servicios;

public class MatriculaServicio
{
    private readonly ApplicationDbContext _db;

    public MatriculaServicio(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ResultadoOperacion> MatricularAsync(string usuarioId, int cursoId)
    {
        await using var transaccion = await _db.Database.BeginTransactionAsync();

        var estudiante = await _db.Estudiantes.Include(item => item.Carrera)
            .FirstOrDefaultAsync(item => item.UsuarioId == usuarioId);
        if (estudiante is null || !estudiante.Activo)
        {
            return ResultadoOperacion.Fallo("La cuenta de estudiante no está activa.");
        }

        if (estudiante.CarreraId is null || estudiante.Carrera is not { Activa: true })
        {
            return ResultadoOperacion.Fallo("Seleccione una carrera activa antes de matricular.");
        }

        var curso = await _db.Cursos.Include(item => item.Carrera).FirstOrDefaultAsync(item => item.Id == cursoId);
        if (curso is null || !curso.Activo || !curso.Carrera.Activa || curso.CarreraId != estudiante.CarreraId)
        {
            return ResultadoOperacion.Fallo("El curso ya no está disponible.");
        }

        var yaActiva = await _db.Matriculas.AnyAsync(matricula =>
            matricula.EstudianteId == estudiante.Id &&
            matricula.CursoId == curso.Id &&
            matricula.Estado == EstadoMatricula.Activa);
        if (yaActiva)
        {
            return ResultadoOperacion.Fallo("Ya está matriculado en este curso. No se creó otra matrícula.");
        }

        var matricula = new Matricula
        {
            EstudianteId = estudiante.Id,
            CursoId = curso.Id,
            Periodo = curso.Periodo,
            Estado = EstadoMatricula.Activa,
            FechaMatricula = DateTime.Now
        };
        _db.Matriculas.Add(matricula);

        try
        {
            await _db.SaveChangesAsync();
            await transaccion.CommitAsync();
        }
        catch (DbUpdateException)
        {
            return ResultadoOperacion.Fallo("Ya está matriculado en este curso. No se creó otra matrícula.");
        }

        return ResultadoOperacion.Ok("Curso matriculado", matricula.Id);
    }

    public async Task<ResultadoOperacion> RetirarAsync(string usuarioId, int matriculaId)
    {
        await using var transaccion = await _db.Database.BeginTransactionAsync();
        var matricula = await _db.Matriculas
            .FirstOrDefaultAsync(item => item.Id == matriculaId && item.Estudiante.UsuarioId == usuarioId);

        if (matricula is null || matricula.Estado != EstadoMatricula.Activa)
        {
            return ResultadoOperacion.Fallo("No tiene una matrícula activa para ese curso.");
        }

        matricula.Estado = EstadoMatricula.Retirada;
        matricula.FechaRetiro = DateTime.Now;
        await _db.SaveChangesAsync();
        await transaccion.CommitAsync();
        return ResultadoOperacion.Ok("Curso retirado", matricula.Id);
    }
}

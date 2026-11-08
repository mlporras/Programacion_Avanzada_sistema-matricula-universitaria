using Microsoft.EntityFrameworkCore;
using ProyectoFinal_PorrasMarco.Data;
using ProyectoFinal_PorrasMarco.Models;

namespace ProyectoFinal_PorrasMarco.Servicios;

public class OfertaServicio
{
    private readonly ApplicationDbContext _db;

    public OfertaServicio(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Estudiante?> EstudiantePorUsuarioAsync(string usuarioId) =>
        _db.Estudiantes.Include(estudiante => estudiante.Carrera)
            .FirstOrDefaultAsync(estudiante => estudiante.UsuarioId == usuarioId);

    public async Task<ResultadoOperacion> GuardarCarreraAsync(string usuarioId, int carreraId)
    {
        var estudiante = await _db.Estudiantes.FirstOrDefaultAsync(item => item.UsuarioId == usuarioId);
        if (estudiante is null || !estudiante.Activo)
        {
            return ResultadoOperacion.Fallo("La cuenta de estudiante no está activa.");
        }

        var carrera = await _db.Carreras.FirstOrDefaultAsync(item => item.Id == carreraId && item.Activa);
        if (carrera is null)
        {
            return ResultadoOperacion.Fallo("Seleccione una carrera activa.", "CarreraId");
        }

        estudiante.CarreraId = carrera.Id;
        await _db.SaveChangesAsync();
        return ResultadoOperacion.Ok("Carrera seleccionada", carrera.Id);
    }

    public async Task<IReadOnlyList<Curso>> CursosDisponiblesAsync(string usuarioId)
    {
        var estudiante = await EstudiantePorUsuarioAsync(usuarioId);
        if (estudiante?.CarreraId is null || estudiante.Carrera is not { Activa: true })
        {
            return [];
        }

        return await _db.Cursos
            .AsNoTracking()
            .Include(curso => curso.Docente)
            .Where(curso => curso.Activo && curso.CarreraId == estudiante.CarreraId)
            .OrderBy(curso => curso.Codigo)
            .ToListAsync();
    }
}

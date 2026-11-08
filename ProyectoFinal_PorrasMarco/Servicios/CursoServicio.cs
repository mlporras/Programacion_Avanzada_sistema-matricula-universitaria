using Microsoft.EntityFrameworkCore;
using ProyectoFinal_PorrasMarco.Data;
using ProyectoFinal_PorrasMarco.Models;
using ProyectoFinal_PorrasMarco.ViewModels;

namespace ProyectoFinal_PorrasMarco.Servicios;

public class CursoServicio
{
    private readonly ApplicationDbContext _db;

    public CursoServicio(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ResultadoOperacion> GuardarAsync(CursoFormVm modelo)
    {
        var codigo = modelo.Codigo.Trim().ToUpperInvariant();
        var id = modelo.Id ?? 0;

        if (await _db.Cursos.AnyAsync(curso => curso.Codigo == codigo && curso.Id != id))
        {
            return ResultadoOperacion.Fallo("El código ya está registrado.", nameof(modelo.Codigo));
        }

        if (modelo.Creditos is < 1 or > 30)
        {
            return ResultadoOperacion.Fallo("Los créditos deben estar entre 1 y 30.", nameof(modelo.Creditos));
        }

        var carrera = await _db.Carreras.FirstOrDefaultAsync(item => item.Id == modelo.CarreraId);
        if (carrera is null || !carrera.Activa)
        {
            return ResultadoOperacion.Fallo("La carrera no está activa. No se guardó la configuración.", nameof(modelo.CarreraId));
        }

        if (modelo.DocenteId.HasValue)
        {
            var docente = await _db.Docentes.FirstOrDefaultAsync(item => item.Id == modelo.DocenteId);
            if (docente is null || !docente.Activo)
            {
                return ResultadoOperacion.Fallo("El docente no está activo. No se guardó la configuración.", nameof(modelo.DocenteId));
            }
        }

        await using var transaccion = await _db.Database.BeginTransactionAsync();
        Curso curso;
        var esNuevo = id == 0;
        if (esNuevo)
        {
            curso = new Curso();
            _db.Cursos.Add(curso);
        }
        else
        {
            var existente = await _db.Cursos.FirstOrDefaultAsync(item => item.Id == id);
            if (existente is null)
            {
                return ResultadoOperacion.Fallo("El curso no existe.");
            }

            curso = existente;
        }

        curso.Codigo = codigo;
        curso.Nombre = modelo.Nombre.Trim();
        curso.Creditos = modelo.Creditos;
        curso.CarreraId = carrera.Id;
        curso.Periodo = modelo.Periodo.Trim();
        curso.Activo = modelo.Activo;
        curso.DocenteId = modelo.DocenteId;

        try
        {
            await _db.SaveChangesAsync();
            await transaccion.CommitAsync();
        }
        catch (DbUpdateException)
        {
            return ResultadoOperacion.Fallo("El código ya está registrado. No se guardó la configuración.", nameof(modelo.Codigo));
        }

        var mensaje = esNuevo
            ? "Curso creado"
            : modelo.DocenteId.HasValue ? "Docente asignado" : "Curso actualizado";
        var detalle = esNuevo && modelo.DocenteId.HasValue ? "Docente asignado" : null;
        return ResultadoOperacion.Ok(mensaje, curso.Id, detalle);
    }
}

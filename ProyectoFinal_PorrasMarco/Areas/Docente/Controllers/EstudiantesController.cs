using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_PorrasMarco.Data;
using ProyectoFinal_PorrasMarco.Models;
using ProyectoFinal_PorrasMarco.Servicios;
using ProyectoFinal_PorrasMarco.ViewModels;

namespace ProyectoFinal_PorrasMarco.Areas.Docente.Controllers;

[Area("Docente")]
[Authorize(Roles = RolesSistema.Docente)]
public class EstudiantesController : ControladorAplicacion
{
    private readonly ApplicationDbContext _db;
    private readonly ListaCursoServicio _listas;

    public EstudiantesController(ApplicationDbContext db, ListaCursoServicio listas)
    {
        _db = db;
        _listas = listas;
    }

    [HttpGet("/docente/estudiantes")]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Estudiantes";
        ViewData["Nav"] = "estudiantes";
        ViewData["Miga"] = "Estudiantes";
        var cursos = await CursosPropiosAsync();
        return View(cursos);
    }

    [HttpGet("/docente/cursos/{id:int}/estudiantes")]
    public async Task<IActionResult> Lista(int id)
    {
        ViewData["Title"] = "Estudiantes del curso";
        ViewData["Nav"] = "estudiantes";
        ViewData["Miga"] = "Mis cursos / Estudiantes";
        var curso = await CursoPropioAsync(id);
        if (curso is null)
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View("/Areas/Docente/Views/Cursos/Denegado.cshtml");
        }

        var activos = await _db.Matriculas.AsNoTracking()
            .Where(matricula => matricula.CursoId == curso.Id && matricula.Estado == EstadoMatricula.Activa)
            .OrderBy(matricula => matricula.Estudiante.Nombre)
            .Select(matricula => new EstudianteListaVm
            {
                MatriculaId = matricula.Id,
                Nombre = matricula.Estudiante.Nombre,
                Correo = matricula.Estudiante.Usuario.Email ?? string.Empty
            })
            .ToListAsync();

        return View(new ListaEstudiantesVm
        {
            CursoId = curso.Id,
            Codigo = curso.Codigo,
            Nombre = curso.Nombre,
            Periodo = Periodos.Etiqueta(curso.Periodo),
            Estudiantes = activos
        });
    }

    [HttpGet("/docente/cursos/{id:int}/estudiantes/agregar")]
    public async Task<IActionResult> Agregar(int id)
    {
        ViewData["Title"] = "Agregar estudiante";
        ViewData["Nav"] = "estudiantes";
        ViewData["Miga"] = "Estudiantes / Agregar";
        var curso = await CursoPropioAsync(id);
        if (curso is null)
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View("/Areas/Docente/Views/Cursos/Denegado.cshtml");
        }

        return View(await FormularioAsync(curso, new JustificacionEstudianteVm()));
    }

    [HttpPost("/docente/cursos/{id:int}/estudiantes/agregar")]
    public async Task<IActionResult> Agregar(int id, JustificacionEstudianteVm modelo)
    {
        ViewData["Title"] = "Agregar estudiante";
        ViewData["Nav"] = "estudiantes";
        ViewData["Miga"] = "Estudiantes / Agregar";
        var curso = await CursoPropioAsync(id);
        if (curso is null)
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View("/Areas/Docente/Views/Cursos/Denegado.cshtml");
        }

        if (!ModelState.IsValid || modelo.EstudianteId is null)
        {
            return View(await FormularioAsync(curso, modelo));
        }

        var resultado = await _listas.AgregarAsync(UsuarioId, id, modelo.EstudianteId.Value, modelo.Justificacion);
        if (!resultado.Exito || resultado.Id is null)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return View(await FormularioAsync(curso, modelo));
        }

        return Redirect($"/docente/cursos/{id}/estudiantes/{resultado.Id}/agregado");
    }

    [HttpGet("/docente/cursos/{id:int}/estudiantes/{matriculaId:int}/agregado")]
    public async Task<IActionResult> Agregado(int id, int matriculaId)
    {
        return await Confirmacion(id, matriculaId, "Estudiante agregado");
    }

    [HttpGet("/docente/cursos/{id:int}/estudiantes/{matriculaId:int}/remover")]
    public async Task<IActionResult> Remover(int id, int matriculaId)
    {
        ViewData["Title"] = "Remover estudiante";
        ViewData["Nav"] = "estudiantes";
        ViewData["Miga"] = "Estudiantes / Remover";
        var curso = await CursoPropioAsync(id);
        if (curso is null)
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View("/Areas/Docente/Views/Cursos/Denegado.cshtml");
        }

        var matricula = await MatriculaActivaAsync(curso.Id, matriculaId);
        if (matricula is null)
        {
            return Redirect($"/docente/cursos/{id}/estudiantes");
        }

        return View(new RemoverEstudianteVm
        {
            CursoId = curso.Id,
            MatriculaId = matricula.Id,
            Curso = $"{curso.Codigo} · {curso.Nombre}",
            Estudiante = matricula.Estudiante.Nombre
        });
    }

    [HttpPost("/docente/cursos/{id:int}/estudiantes/{matriculaId:int}/remover")]
    public async Task<IActionResult> Remover(int id, int matriculaId, RemoverEstudianteVm modelo)
    {
        ViewData["Title"] = "Remover estudiante";
        ViewData["Nav"] = "estudiantes";
        var curso = await CursoPropioAsync(id);
        if (curso is null)
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View("/Areas/Docente/Views/Cursos/Denegado.cshtml");
        }

        modelo.CursoId = curso.Id;
        modelo.MatriculaId = matriculaId;
        modelo.Curso = $"{curso.Codigo} · {curso.Nombre}";
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var resultado = await _listas.RetirarAsync(UsuarioId, id, matriculaId, modelo.Justificacion);
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return View(modelo);
        }

        return Redirect($"/docente/cursos/{id}/estudiantes/{matriculaId}/removido");
    }

    [HttpGet("/docente/cursos/{id:int}/estudiantes/{matriculaId:int}/removido")]
    public async Task<IActionResult> Removido(int id, int matriculaId)
    {
        return await Confirmacion(id, matriculaId, "Estudiante removido");
    }

    private async Task<IActionResult> Confirmacion(int cursoId, int matriculaId, string mensaje)
    {
        ViewData["Title"] = mensaje;
        ViewData["Nav"] = "estudiantes";
        ViewData["Miga"] = $"Estudiantes / {mensaje}";
        var curso = await CursoPropioAsync(cursoId);
        if (curso is null)
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View("/Areas/Docente/Views/Cursos/Denegado.cshtml");
        }

        var matricula = await _db.Matriculas.AsNoTracking().Include(item => item.Estudiante)
            .FirstOrDefaultAsync(item => item.Id == matriculaId && item.CursoId == curso.Id);
        if (matricula is null)
        {
            return Redirect($"/docente/cursos/{cursoId}/estudiantes");
        }

        return View("Confirmacion", new ConfirmacionListaVm
        {
            CursoId = curso.Id,
            Mensaje = mensaje,
            Estudiante = matricula.Estudiante.Nombre,
            Curso = $"{curso.Codigo} · {curso.Nombre}"
        });
    }

    private async Task<JustificacionEstudianteVm> FormularioAsync(Curso curso, JustificacionEstudianteVm modelo)
    {
        var activos = await _db.Matriculas.Where(matricula => matricula.CursoId == curso.Id && matricula.Estado == EstadoMatricula.Activa)
            .Select(matricula => matricula.EstudianteId)
            .ToListAsync();
        modelo.CursoId = curso.Id;
        modelo.Curso = $"{curso.Codigo} · {curso.Nombre}";
        modelo.Opciones = await _db.Estudiantes.AsNoTracking()
            .Where(estudiante => estudiante.Activo && estudiante.CarreraId == curso.CarreraId && !activos.Contains(estudiante.Id))
            .OrderBy(estudiante => estudiante.Nombre)
            .Select(estudiante => new EstudianteOpcionVm { Id = estudiante.Id, Nombre = estudiante.Nombre })
            .ToListAsync();
        return modelo;
    }

    private Task<Curso?> CursoPropioAsync(int id) =>
        _db.Cursos.AsNoTracking().Include(curso => curso.Docente)
            .FirstOrDefaultAsync(curso => curso.Id == id && curso.Activo && curso.Docente != null && curso.Docente.UsuarioId == UsuarioId);

    private Task<List<CursoDocenteFilaVm>> CursosPropiosAsync() =>
        _db.Cursos.AsNoTracking()
            .Where(curso => curso.Activo && curso.Docente != null && curso.Docente.UsuarioId == UsuarioId)
            .OrderBy(curso => curso.Codigo)
            .Select(curso => new CursoDocenteFilaVm
            {
                Id = curso.Id,
                Codigo = curso.Codigo,
                Nombre = curso.Nombre,
                Periodo = curso.Periodo,
                Estudiantes = curso.Matriculas.Count(matricula => matricula.Estado == EstadoMatricula.Activa)
            })
            .ToListAsync();

    private Task<Matricula?> MatriculaActivaAsync(int cursoId, int matriculaId) =>
        _db.Matriculas.AsNoTracking().Include(item => item.Estudiante)
            .FirstOrDefaultAsync(item => item.Id == matriculaId && item.CursoId == cursoId && item.Estado == EstadoMatricula.Activa);
}

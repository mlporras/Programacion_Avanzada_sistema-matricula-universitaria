using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_PorrasMarco.Data;
using ProyectoFinal_PorrasMarco.Models;
using ProyectoFinal_PorrasMarco.ViewModels;

namespace ProyectoFinal_PorrasMarco.Areas.Docente.Controllers;

[Area("Docente")]
[Authorize(Roles = RolesSistema.Docente)]
public class CursosController : ControladorAplicacion
{
    private readonly ApplicationDbContext _db;

    public CursosController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("/docente")]
    [HttpGet("/docente/cursos")]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Mis cursos";
        ViewData["Nav"] = "cursos";
        ViewData["Miga"] = "Mis cursos";
        var cursos = await _db.Cursos.AsNoTracking()
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
        return View(cursos);
    }

    [HttpGet("/docente/cursos/{id:int}")]
    public async Task<IActionResult> Detalle(int id)
    {
        ViewData["Title"] = "Curso asignado";
        ViewData["Nav"] = "cursos";
        ViewData["Miga"] = "Mis cursos / Detalle";
        var curso = await _db.Cursos.AsNoTracking()
            .Include(item => item.Carrera)
            .Include(item => item.Docente)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (curso is null || curso.Docente is null || curso.Docente.UsuarioId != UsuarioId)
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View("Denegado");
        }

        return View(new DetalleCursoDocenteVm
        {
            Codigo = curso.Codigo,
            Nombre = curso.Nombre,
            Carrera = curso.Carrera.Nombre,
            Periodo = curso.Periodo,
            Estado = curso.Activo ? "Activo" : "Inactivo",
            Estudiantes = await _db.Matriculas.CountAsync(matricula => matricula.CursoId == curso.Id && matricula.Estado == EstadoMatricula.Activa)
        });
    }
}

[Area("Docente")]
[Authorize(Roles = RolesSistema.Docente)]
public class PendienteController : Controller
{
    [HttpGet("/docente/estudiantes")]
    public IActionResult Estudiantes() => Vista("Estudiantes", "estudiantes");

    [HttpGet("/docente/calificaciones")]
    public IActionResult Calificaciones() => Vista("Calificaciones", "calificaciones");

    [HttpGet("/docente/reportes")]
    public IActionResult Reportes() => Vista("Reportes", "reportes");

    private IActionResult Vista(string nombre, string nav)
    {
        ViewData["Title"] = nombre;
        ViewData["Nav"] = nav;
        ViewData["Miga"] = $"Mis cursos / {nombre}";
        return View("Index", nombre);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_PorrasMarco.Data;
using ProyectoFinal_PorrasMarco.Models;
using ProyectoFinal_PorrasMarco.Servicios;
using ProyectoFinal_PorrasMarco.ViewModels;

namespace ProyectoFinal_PorrasMarco.Areas.Estudiante.Controllers;

[Area("Estudiante")]
[Authorize(Roles = RolesSistema.Estudiante)]
public class InicioController : ControladorAplicacion
{
    private readonly OfertaServicio _oferta;
    private readonly ApplicationDbContext _db;

    public InicioController(OfertaServicio oferta, ApplicationDbContext db)
    {
        _oferta = oferta;
        _db = db;
    }

    [HttpGet("/estudiante")]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Panel del estudiante";
        ViewData["Nav"] = "inicio";
        ViewData["Miga"] = "Inicio";
        var estudiante = await _oferta.EstudiantePorUsuarioAsync(UsuarioId);
        var cursos = estudiante is null
            ? 0
            : await _db.Matriculas.CountAsync(matricula => matricula.EstudianteId == estudiante.Id && matricula.Estado == EstadoMatricula.Activa);
        return View(new PanelEstudianteVm
        {
            Carrera = estudiante?.Carrera?.Nombre ?? "Sin carrera seleccionada",
            CursosActivos = cursos
        });
    }
}

[Area("Estudiante")]
[Authorize(Roles = RolesSistema.Estudiante)]
public class CarreraController : ControladorAplicacion
{
    private readonly ApplicationDbContext _db;
    private readonly OfertaServicio _oferta;

    public CarreraController(ApplicationDbContext db, OfertaServicio oferta)
    {
        _db = db;
        _oferta = oferta;
    }

    [HttpGet("/estudiante/carrera")]
    public async Task<IActionResult> Seleccionar()
    {
        ViewData["Title"] = "Seleccionar carrera";
        ViewData["Nav"] = "carrera";
        ViewData["Miga"] = "Inicio / Carrera";
        var estudiante = await _oferta.EstudiantePorUsuarioAsync(UsuarioId);
        var modelo = await CrearModeloAsync(estudiante?.CarreraId);
        return View(modelo);
    }

    [HttpPost("/estudiante/carrera")]
    public async Task<IActionResult> Seleccionar(SeleccionarCarreraVm modelo)
    {
        ViewData["Title"] = "Seleccionar carrera";
        ViewData["Nav"] = "carrera";
        ViewData["Miga"] = "Inicio / Carrera";
        if (!ModelState.IsValid || modelo.CarreraId is null)
        {
            return View(await CrearModeloAsync(modelo.CarreraId));
        }

        var resultado = await _oferta.GuardarCarreraAsync(UsuarioId, modelo.CarreraId.Value);
        if (!resultado.Exito || resultado.Id is null)
        {
            CopiarErrores(resultado);
            return View(await CrearModeloAsync(modelo.CarreraId));
        }

        TempData["Mensaje"] = resultado.Mensaje;
        return RedirectToAction(nameof(Confirmacion));
    }

    [HttpGet("/estudiante/carrera/confirmacion")]
    public async Task<IActionResult> Confirmacion()
    {
        ViewData["Title"] = "Carrera seleccionada";
        ViewData["Nav"] = "carrera";
        ViewData["Miga"] = "Carrera / Confirmación";
        var estudiante = await _oferta.EstudiantePorUsuarioAsync(UsuarioId);
        if (estudiante?.Carrera is null)
        {
            return RedirectToAction(nameof(Seleccionar));
        }

        ViewData["Mensaje"] = TempData["Mensaje"];
        return View(estudiante.Carrera);
    }

    private async Task<SeleccionarCarreraVm> CrearModeloAsync(int? carreraId)
    {
        var carreras = await _db.Carreras.AsNoTracking().Where(carrera => carrera.Activa).OrderBy(carrera => carrera.Nombre).ToListAsync();
        return new SeleccionarCarreraVm
        {
            CarreraId = carreraId,
            Carreras = carreras.Select(carrera => new SelectListItem
            {
                Value = carrera.Id.ToString(),
                Text = $"{carrera.Codigo} · {carrera.Nombre}",
                Selected = carrera.Id == carreraId
            }).ToList()
        };
    }
}

[Area("Estudiante")]
[Authorize(Roles = RolesSistema.Estudiante)]
public class OfertaController : ControladorAplicacion
{
    private readonly ApplicationDbContext _db;
    private readonly OfertaServicio _oferta;
    private readonly MatriculaServicio _matriculas;

    public OfertaController(ApplicationDbContext db, OfertaServicio oferta, MatriculaServicio matriculas)
    {
        _db = db;
        _oferta = oferta;
        _matriculas = matriculas;
    }

    [HttpGet("/estudiante/cursos")]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Cursos disponibles";
        ViewData["Nav"] = "oferta";
        ViewData["Miga"] = "Inicio / Cursos disponibles";
        var estudiante = await _oferta.EstudiantePorUsuarioAsync(UsuarioId);
        if (estudiante?.CarreraId is null || estudiante.Carrera is not { Activa: true })
        {
            return View(new OfertaEstudianteVm { SinCarrera = true });
        }

        var activos = await IdsMatriculadosAsync(estudiante.Id);
        var cursos = await _oferta.CursosDisponiblesAsync(UsuarioId);
        return View(new OfertaEstudianteVm
        {
            Carrera = estudiante.Carrera.Nombre,
            Cursos = cursos.Select(curso => new CursoOfertaFilaVm
            {
                Id = curso.Id,
                Codigo = curso.Codigo,
                Nombre = curso.Nombre,
                Periodo = curso.Periodo,
                Docente = curso.Docente?.Nombre ?? "Sin docente asignado",
                YaMatriculado = activos.Contains(curso.Id)
            }).ToList()
        });
    }

    [HttpGet("/estudiante/cursos/{id:int}")]
    public async Task<IActionResult> Detalle(int id)
    {
        ViewData["Title"] = "Curso disponible";
        ViewData["Nav"] = "oferta";
        ViewData["Miga"] = "Cursos disponibles / Detalle";
        var modelo = await CargarDisponibleAsync(id);
        if (modelo is null)
        {
            return View("NoDisponible");
        }

        return View(modelo);
    }

    [HttpGet("/estudiante/cursos/{id:int}/confirmar")]
    public async Task<IActionResult> Confirmar(int id)
    {
        ViewData["Title"] = "Confirmar matrícula";
        ViewData["Nav"] = "oferta";
        ViewData["Miga"] = "Cursos disponibles / Confirmación";
        var modelo = await CargarDisponibleAsync(id);
        if (modelo is null)
        {
            return View("NoDisponible");
        }

        if (modelo.YaMatriculado)
        {
            modelo.Error = "Ya está matriculado en este curso. No se creó otra matrícula.";
        }

        return View(modelo);
    }

    [HttpPost("/estudiante/cursos/{id:int}/matricular")]
    public async Task<IActionResult> Matricular(int id)
    {
        var resultado = await _matriculas.MatricularAsync(UsuarioId, id);
        if (!resultado.Exito || resultado.Id is null)
        {
            var modelo = await CargarDisponibleAsync(id) ?? new DetalleCursoDisponibleVm { Id = id, Nombre = "Curso no disponible" };
            modelo.Error = resultado.Mensaje;
            ViewData["Title"] = "Matrícula";
            ViewData["Nav"] = "oferta";
            ViewData["Miga"] = "Cursos disponibles / Error";
            return View("Confirmar", modelo);
        }

        TempData["Mensaje"] = resultado.Mensaje;
        return Redirect($"/estudiante/mis-cursos/{resultado.Id}/resultado");
    }

    private async Task<DetalleCursoDisponibleVm?> CargarDisponibleAsync(int id)
    {
        var estudiante = await _oferta.EstudiantePorUsuarioAsync(UsuarioId);
        if (estudiante?.CarreraId is null)
        {
            return null;
        }

        var curso = await _db.Cursos.AsNoTracking().Include(item => item.Docente)
            .FirstOrDefaultAsync(item => item.Id == id && item.Activo && item.CarreraId == estudiante.CarreraId);
        if (curso is null)
        {
            return null;
        }

        var activos = await IdsMatriculadosAsync(estudiante.Id);
        return new DetalleCursoDisponibleVm
        {
            Id = curso.Id,
            Codigo = curso.Codigo,
            Nombre = curso.Nombre,
            Periodo = curso.Periodo,
            Docente = curso.Docente?.Nombre ?? "Sin docente asignado",
            Creditos = curso.Creditos?.ToString(),
            YaMatriculado = activos.Contains(curso.Id)
        };
    }

    private async Task<HashSet<int>> IdsMatriculadosAsync(int estudianteId)
    {
        var ids = await _db.Matriculas.AsNoTracking()
            .Where(matricula => matricula.EstudianteId == estudianteId && matricula.Estado == EstadoMatricula.Activa)
            .Select(matricula => matricula.CursoId)
            .ToListAsync();
        return ids.ToHashSet();
    }
}

[Area("Estudiante")]
[Authorize(Roles = RolesSistema.Estudiante)]
public class MisCursosController : ControladorAplicacion
{
    private readonly ApplicationDbContext _db;
    private readonly MatriculaServicio _matriculas;

    public MisCursosController(ApplicationDbContext db, MatriculaServicio matriculas)
    {
        _db = db;
        _matriculas = matriculas;
    }

    [HttpGet("/estudiante/mis-cursos")]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Mis cursos";
        ViewData["Nav"] = "miscursos";
        ViewData["Miga"] = "Inicio / Mis cursos";
        var matriculas = await ConsultarPropiasAsync();
        return View(new MisCursosVm
        {
            Activos = matriculas.Where(item => item.Estado == "Activa").ToList(),
            Retiros = matriculas.Where(item => item.Estado == "Retirada").ToList()
        });
    }

    [HttpGet("/estudiante/mis-cursos/{id:int}")]
    public async Task<IActionResult> Detalle(int id)
    {
        ViewData["Title"] = "Curso matriculado";
        ViewData["Nav"] = "miscursos";
        ViewData["Miga"] = "Mis cursos / Detalle";
        var modelo = await CargarPropiaAsync(id);
        if (modelo is null)
        {
            return View("NoEncontrada");
        }

        return View(modelo);
    }

    [HttpGet("/estudiante/mis-cursos/{id:int}/resultado")]
    public async Task<IActionResult> Resultado(int id)
    {
        ViewData["Title"] = "Curso matriculado";
        ViewData["Nav"] = "miscursos";
        ViewData["Miga"] = "Mis cursos / Confirmación";
        var modelo = await CargarPropiaAsync(id);
        if (modelo is null)
        {
            return View("NoEncontrada");
        }

        modelo.Mensaje = TempData["Mensaje"] as string ?? "Curso matriculado";
        modelo.Detalle = "Puede consultarlo en Mis cursos.";
        return View(modelo);
    }

    [HttpGet("/estudiante/mis-cursos/{id:int}/retirar")]
    public async Task<IActionResult> Retirar(int id)
    {
        ViewData["Title"] = "Retirar curso";
        ViewData["Nav"] = "miscursos";
        ViewData["Miga"] = "Mis cursos / Retiro";
        var modelo = await CargarPropiaAsync(id, soloActiva: true);
        if (modelo is null)
        {
            return View("NoEncontrada");
        }

        return View(modelo);
    }

    [HttpPost("/estudiante/mis-cursos/{id:int}/retirar")]
    public async Task<IActionResult> RetirarConfirmado(int id)
    {
        var resultado = await _matriculas.RetirarAsync(UsuarioId, id);
        if (!resultado.Exito)
        {
            ViewData["Title"] = "Retirar curso";
            ViewData["Nav"] = "miscursos";
            var modelo = await CargarPropiaAsync(id) ?? new DetalleMatriculaVm();
            modelo.Mensaje = resultado.Mensaje;
            return View("Retirar", modelo);
        }

        TempData["Mensaje"] = resultado.Mensaje;
        return RedirectToAction(nameof(Retirado), new { id });
    }

    [HttpGet("/estudiante/mis-cursos/{id:int}/retirado")]
    public async Task<IActionResult> Retirado(int id)
    {
        ViewData["Title"] = "Curso retirado";
        ViewData["Nav"] = "miscursos";
        ViewData["Miga"] = "Mis cursos / Confirmación";
        var modelo = await CargarPropiaAsync(id);
        if (modelo is null)
        {
            return View("NoEncontrada");
        }

        modelo.Mensaje = TempData["Mensaje"] as string ?? "Curso retirado";
        modelo.Detalle = "El curso ya no aparece como activo en Mis cursos.";
        return View(modelo);
    }

    private async Task<List<CursoMatriculadoFilaVm>> ConsultarPropiasAsync()
    {
        return await _db.Matriculas.AsNoTracking()
            .Where(matricula => matricula.Estudiante.UsuarioId == UsuarioId)
            .OrderBy(matricula => matricula.Curso.Codigo)
            .Select(matricula => new CursoMatriculadoFilaVm
            {
                MatriculaId = matricula.Id,
                Codigo = matricula.Curso.Codigo,
                Nombre = matricula.Curso.Nombre,
                Periodo = matricula.Periodo,
                Docente = matricula.Curso.Docente != null ? matricula.Curso.Docente.Nombre : "Sin docente asignado",
                Estado = matricula.Estado == EstadoMatricula.Activa ? "Activa" : "Retirada"
            })
            .ToListAsync();
    }

    private async Task<DetalleMatriculaVm?> CargarPropiaAsync(int id, bool soloActiva = false)
    {
        var matricula = await _db.Matriculas.AsNoTracking()
            .Include(item => item.Curso).ThenInclude(curso => curso.Carrera)
            .Include(item => item.Curso).ThenInclude(curso => curso.Docente)
            .FirstOrDefaultAsync(item => item.Id == id && item.Estudiante.UsuarioId == UsuarioId);
        if (matricula is null || (soloActiva && matricula.Estado != EstadoMatricula.Activa))
        {
            return null;
        }

        return new DetalleMatriculaVm
        {
            MatriculaId = matricula.Id,
            Codigo = matricula.Curso.Codigo,
            Nombre = matricula.Curso.Nombre,
            Carrera = matricula.Curso.Carrera.Nombre,
            Periodo = matricula.Periodo,
            Docente = matricula.Curso.Docente?.Nombre ?? "Sin docente asignado",
            Estado = matricula.Estado == EstadoMatricula.Activa ? "Activa" : "Retirada",
            Creditos = matricula.Curso.Creditos?.ToString()
        };
    }
}

[Area("Estudiante")]
[Authorize(Roles = RolesSistema.Estudiante)]
public class CalificacionesController : ControladorAplicacion
{
    private readonly ApplicationDbContext _db;
    private readonly OfertaServicio _oferta;
    private readonly ReporteServicio _reportes;

    public CalificacionesController(ApplicationDbContext db, OfertaServicio oferta, ReporteServicio reportes)
    {
        _db = db;
        _oferta = oferta;
        _reportes = reportes;
    }

    [HttpGet("/estudiante/calificaciones")]
    public async Task<IActionResult> Index(string? periodo, int? estudianteId)
    {
        ViewData["Title"] = "Reporte personal";
        ViewData["Nav"] = "calificaciones";
        ViewData["Miga"] = "Inicio / Calificaciones";
        var estudiante = await _oferta.EstudiantePorUsuarioAsync(UsuarioId);
        if (estudianteId.HasValue && (estudiante is null || estudiante.Id != estudianteId.Value))
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View("Denegado");
        }

        var reporte = await _reportes.ConsultarPropioAsync(UsuarioId, periodo);
        if (reporte is null)
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View("Denegado");
        }

        return View(reporte);
    }

    [HttpPost("/estudiante/calificaciones/modificar")]
    public IActionResult Modificar()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        ViewData["Title"] = "Acceso no autorizado";
        ViewData["Nav"] = "calificaciones";
        return View("Denegado");
    }
}

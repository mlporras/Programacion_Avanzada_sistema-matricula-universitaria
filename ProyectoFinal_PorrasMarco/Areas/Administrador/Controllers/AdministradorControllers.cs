using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_PorrasMarco.Data;
using ProyectoFinal_PorrasMarco.Models;
using ProyectoFinal_PorrasMarco.Servicios;
using ProyectoFinal_PorrasMarco.ViewModels;

namespace ProyectoFinal_PorrasMarco.Areas.Administrador.Controllers;

[Area("Administrador")]
[Authorize(Roles = RolesSistema.Administrador)]
public class PanelController : Controller
{
    private readonly ApplicationDbContext _db;

    public PanelController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("/administrador")]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Panel del administrador";
        ViewData["Nav"] = "panel";
        ViewData["Miga"] = "Panel";
        var modelo = new PanelAdministradorVm
        {
            Usuarios = await _db.Users.CountAsync(),
            CursosActivos = await _db.Carreras.CountAsync()
        };
        return View(modelo);
    }
}

[Area("Administrador")]
[Authorize(Roles = RolesSistema.Administrador)]
public class CarrerasController : ControladorAplicacion
{
    private readonly ApplicationDbContext _db;
    private readonly CarreraServicio _carreras;

    public CarrerasController(ApplicationDbContext db, CarreraServicio carreras)
    {
        _db = db;
        _carreras = carreras;
    }

    [HttpGet("/administrador/carreras")]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Carreras";
        ViewData["Nav"] = "carreras";
        ViewData["Miga"] = "Panel / Carreras";

        var carreras = await _db.Carreras.AsNoTracking().OrderBy(carrera => carrera.Nombre).ToListAsync();
        var modelo = new OfertaAdministradorVm
        {
            Carreras = carreras.Select(carrera => new CarreraFilaVm
            {
                Id = carrera.Id,
                Codigo = carrera.Codigo,
                Nombre = carrera.Nombre,
                Activa = carrera.Activa,
                EsDemostracion = carrera.EsDatoDemostracion
            }).ToList()
        };
        return View(modelo);
    }

    [HttpGet("/administrador/carreras/nueva")]
    public IActionResult Crear()
    {
        PrepararCarrera("Nueva carrera", false);
        return View("Carrera", new CarreraFormVm());
    }

    [HttpPost("/administrador/carreras/nueva")]
    public Task<IActionResult> Crear(CarreraFormVm modelo) => GuardarCarrera(modelo, false);

    [HttpGet("/administrador/carreras/{id:int}/editar")]
    public async Task<IActionResult> Editar(int id)
    {
        var carrera = await _db.Carreras.FindAsync(id);
        if (carrera is null)
        {
            return NotFound();
        }

        PrepararCarrera("Editar carrera", true);
        return View("Carrera", new CarreraFormVm
        {
            Id = carrera.Id,
            Codigo = carrera.Codigo,
            Nombre = carrera.Nombre,
            Activa = carrera.Activa
        });
    }

    [HttpPost("/administrador/carreras/{id:int}/editar")]
    public Task<IActionResult> Editar(int id, CarreraFormVm modelo)
    {
        modelo.Id = id;
        return GuardarCarrera(modelo, true);
    }

    [HttpGet("/administrador/carreras/confirmacion/{id:int}")]
    public async Task<IActionResult> CarreraConfirmada(int id)
    {
        ViewData["Title"] = "Carrera guardada";
        ViewData["Nav"] = "carreras";
        ViewData["Miga"] = "Carreras / Confirmación";
        var carrera = await _db.Carreras.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
        if (carrera is null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(new ConfirmacionCarreraVm
        {
            Codigo = carrera.Codigo,
            Nombre = carrera.Nombre,
            Estado = carrera.Activa ? "Activa" : "Inactiva",
            Mensaje = TempData["Mensaje"] as string
        });
    }

    private async Task<IActionResult> GuardarCarrera(CarreraFormVm modelo, bool edicion)
    {
        PrepararCarrera(edicion ? "Editar carrera" : "Nueva carrera", edicion);
        if (!ModelState.IsValid)
        {
            return View("Carrera", modelo);
        }

        var resultado = await _carreras.GuardarAsync(modelo);
        if (!resultado.Exito || resultado.Id is null)
        {
            CopiarErrores(resultado);
            return View("Carrera", modelo);
        }

        TempData["Mensaje"] = resultado.Mensaje;
        return RedirectToAction(nameof(CarreraConfirmada), new { id = resultado.Id });
    }

    private void PrepararCarrera(string titulo, bool edicion)
    {
        ViewData["Title"] = titulo;
        ViewData["Nav"] = "carreras";
        ViewData["Miga"] = edicion ? "Carreras / Editar carrera" : "Carreras / Nueva carrera";
    }
}

[Area("Administrador")]
[Authorize(Roles = RolesSistema.Administrador)]
public class PendienteController : Controller
{
    [HttpGet("/administrador/matriculas")]
    public IActionResult Matriculas() => Vista("Matrículas", "matriculas");

    [HttpGet("/administrador/calificaciones")]
    public IActionResult Calificaciones() => Vista("Calificaciones", "calificaciones");

    [HttpGet("/administrador/auditoria")]
    public IActionResult Auditoria() => Vista("Auditoría", "auditoria");

    private IActionResult Vista(string nombre, string nav)
    {
        ViewData["Title"] = nombre;
        ViewData["Nav"] = nav;
        ViewData["Miga"] = $"Panel / {nombre}";
        return View("Index", nombre);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
            CursosActivos = await _db.Cursos.CountAsync(curso => curso.Activo),
            MatriculasActivas = await _db.Matriculas.CountAsync(matricula => matricula.Estado == EstadoMatricula.Activa)
        };
        return View(modelo);
    }
}

[Area("Administrador")]
[Authorize(Roles = RolesSistema.Administrador)]
public class UsuariosController : ControladorAplicacion
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _usuarios;
    private readonly UsuarioServicio _servicio;

    public UsuariosController(ApplicationDbContext db, UserManager<ApplicationUser> usuarios, UsuarioServicio servicio)
    {
        _db = db;
        _usuarios = usuarios;
        _servicio = servicio;
    }

    [HttpGet("/administrador/usuarios")]
    public async Task<IActionResult> Index(string? busqueda, string? rol, string? estado)
    {
        ViewData["Title"] = "Usuarios y roles";
        ViewData["Nav"] = "usuarios";
        ViewData["Miga"] = "Panel / Usuarios";
        ViewData["Busqueda"] = busqueda;
        ViewData["Rol"] = rol;
        ViewData["Estado"] = estado;

        var consulta = _db.Users.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var texto = busqueda.Trim();
            consulta = consulta.Where(usuario => usuario.NombreCompleto.Contains(texto) || (usuario.Email != null && usuario.Email.Contains(texto)));
        }

        var personas = await consulta.OrderBy(usuario => usuario.NombreCompleto).ToListAsync();
        var filas = new List<UsuarioFilaVm>();
        foreach (var persona in personas)
        {
            var roles = await _usuarios.GetRolesAsync(persona);
            var rolPersona = roles.FirstOrDefault() ?? "Sin rol";
            var activo = !(persona.LockoutEnd > DateTimeOffset.UtcNow);
            if (!string.IsNullOrWhiteSpace(rol) && !string.Equals(rolPersona, rol, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (estado == "activo" && !activo)
            {
                continue;
            }

            if (estado == "inactivo" && activo)
            {
                continue;
            }

            filas.Add(new UsuarioFilaVm
            {
                Id = persona.Id,
                Nombre = persona.NombreCompleto,
                Correo = persona.Email ?? string.Empty,
                Rol = rolPersona,
                Activo = activo
            });
        }

        return View(filas);
    }

    [HttpGet("/administrador/usuarios/nuevo")]
    public IActionResult Crear()
    {
        PrepararVista();
        return View(new NuevoUsuarioVm());
    }

    [HttpPost("/administrador/usuarios/nuevo")]
    public async Task<IActionResult> Crear(NuevoUsuarioVm modelo)
    {
        PrepararVista();
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var resultado = await _servicio.CrearPorAdministradorAsync(modelo);
        if (!resultado.Exito || string.IsNullOrEmpty(resultado.Detalle))
        {
            CopiarErrores(resultado);
            return View(modelo);
        }

        TempData["Mensaje"] = resultado.Mensaje;
        TempData["Detalle"] = "No se creó una cuenta duplicada. El correo no estaba registrado.";
        return RedirectToAction(nameof(Creado), new { id = resultado.Detalle });
    }

    [HttpGet("/administrador/usuarios/{id}/creado")]
    public async Task<IActionResult> Creado(string id)
    {
        ViewData["Title"] = "Usuario creado";
        ViewData["Nav"] = "usuarios";
        ViewData["Miga"] = "Usuarios / Confirmación";
        var usuario = await _usuarios.FindByIdAsync(id);
        if (usuario is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var roles = await _usuarios.GetRolesAsync(usuario);
        var modelo = new UsuarioCreadoVm
        {
            Nombre = usuario.NombreCompleto,
            Correo = usuario.Email ?? string.Empty,
            Rol = roles.FirstOrDefault() ?? "Sin rol",
            Estado = usuario.LockoutEnd > DateTimeOffset.UtcNow ? "Inactivo" : "Activo",
            Mensaje = TempData["Mensaje"] as string,
            Detalle = TempData["Detalle"] as string
        };
        return View(modelo);
    }

    private void PrepararVista()
    {
        ViewData["Title"] = "Nuevo usuario";
        ViewData["Nav"] = "usuarios";
        ViewData["Miga"] = "Panel / Usuarios / Nuevo usuario";
    }
}

[Area("Administrador")]
[Authorize(Roles = RolesSistema.Administrador)]
public class CarrerasController : ControladorAplicacion
{
    private readonly ApplicationDbContext _db;
    private readonly CarreraServicio _carreras;
    private readonly CursoServicio _cursos;

    public CarrerasController(ApplicationDbContext db, CarreraServicio carreras, CursoServicio cursos)
    {
        _db = db;
        _carreras = carreras;
        _cursos = cursos;
    }

    [HttpGet("/administrador/carreras")]
    public async Task<IActionResult> Index(int? carreraId, string? periodo)
    {
        ViewData["Title"] = "Carreras y cursos";
        ViewData["Nav"] = "carreras";
        ViewData["Miga"] = "Panel / Carreras y cursos";

        var carreras = await _db.Carreras.AsNoTracking().OrderBy(carrera => carrera.Nombre).ToListAsync();
        var periodos = await _db.Cursos.AsNoTracking().Select(curso => curso.Periodo).Distinct().OrderBy(item => item).ToListAsync();
        var consulta = _db.Cursos.AsNoTracking().Include(curso => curso.Carrera).Include(curso => curso.Docente).AsQueryable();
        if (carreraId.HasValue)
        {
            consulta = consulta.Where(curso => curso.CarreraId == carreraId);
        }

        if (!string.IsNullOrWhiteSpace(periodo))
        {
            consulta = consulta.Where(curso => curso.Periodo == periodo);
        }

        var cursos = await consulta.OrderBy(curso => curso.Codigo).ToListAsync();
        var modelo = new OfertaAdministradorVm
        {
            CarreraId = carreraId,
            Periodo = periodo,
            Periodos = periodos,
            Carreras = carreras.Select(carrera => new CarreraFilaVm
            {
                Id = carrera.Id,
                Codigo = carrera.Codigo,
                Nombre = carrera.Nombre,
                Activa = carrera.Activa,
                EsDemostracion = carrera.EsDatoDemostracion
            }).ToList(),
            Cursos = cursos.Select(curso => new CursoFilaVm
            {
                Id = curso.Id,
                Codigo = curso.Codigo,
                Nombre = curso.Nombre,
                Carrera = curso.Carrera.Nombre,
                Periodo = curso.Periodo,
                Docente = curso.Docente?.Nombre ?? "Sin docente",
                Activo = curso.Activo,
                EsDemostracion = curso.EsDatoDemostracion
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
        ViewData["Miga"] = "Carreras y cursos / Confirmación";
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

    [HttpGet("/administrador/cursos/nuevo")]
    public async Task<IActionResult> CrearCurso()
    {
        PrepararCurso("Curso", false);
        return View("Curso", await CompletarCursoAsync(new CursoFormVm()));
    }

    [HttpPost("/administrador/cursos/nuevo")]
    public async Task<IActionResult> CrearCurso(CursoFormVm modelo)
    {
        modelo.Id = null;
        return await GuardarCurso(modelo, false);
    }

    [HttpGet("/administrador/cursos/{id:int}/editar")]
    public async Task<IActionResult> EditarCurso(int id)
    {
        var curso = await _db.Cursos.FirstOrDefaultAsync(item => item.Id == id);
        if (curso is null)
        {
            return NotFound();
        }

        PrepararCurso("Curso", true);
        return View("Curso", await CompletarCursoAsync(new CursoFormVm
        {
            Id = curso.Id,
            Codigo = curso.Codigo,
            Nombre = curso.Nombre,
            Creditos = curso.Creditos,
            CarreraId = curso.CarreraId,
            Periodo = curso.Periodo,
            Activo = curso.Activo,
            DocenteId = curso.DocenteId
        }));
    }

    [HttpPost("/administrador/cursos/{id:int}/editar")]
    public async Task<IActionResult> EditarCurso(int id, CursoFormVm modelo)
    {
        modelo.Id = id;
        return await GuardarCurso(modelo, true);
    }

    [HttpGet("/administrador/cursos/{id:int}/confirmacion")]
    public async Task<IActionResult> CursoConfirmado(int id)
    {
        ViewData["Title"] = "Curso guardado";
        ViewData["Nav"] = "carreras";
        ViewData["Miga"] = "Carreras y cursos / Confirmación";
        var curso = await _db.Cursos.AsNoTracking().Include(item => item.Carrera).Include(item => item.Docente)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (curso is null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(new ConfirmacionCursoVm
        {
            Codigo = curso.Codigo,
            Nombre = curso.Nombre,
            Carrera = curso.Carrera.Nombre,
            Periodo = curso.Periodo,
            Estado = curso.Activo ? "Activo" : "Inactivo",
            Docente = curso.Docente?.Nombre ?? "Sin docente",
            Mensaje = TempData["Mensaje"] as string,
            Detalle = TempData["Detalle"] as string
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

    private async Task<IActionResult> GuardarCurso(CursoFormVm modelo, bool edicion)
    {
        PrepararCurso("Curso", edicion);
        if (!ModelState.IsValid)
        {
            return View("Curso", await CompletarCursoAsync(modelo));
        }

        var resultado = await _cursos.GuardarAsync(modelo);
        if (!resultado.Exito || resultado.Id is null)
        {
            CopiarErrores(resultado);
            return View("Curso", await CompletarCursoAsync(modelo));
        }

        TempData["Mensaje"] = resultado.Mensaje;
        TempData["Detalle"] = resultado.Detalle;
        return RedirectToAction(nameof(CursoConfirmado), new { id = resultado.Id });
    }

    private async Task<CursoFormVm> CompletarCursoAsync(CursoFormVm modelo)
    {
        var carreras = await _db.Carreras.AsNoTracking().OrderBy(carrera => carrera.Nombre).ToListAsync();
        var docentes = await _db.Docentes.AsNoTracking().OrderBy(docente => docente.Nombre).ToListAsync();
        modelo.Carreras = carreras
            .Where(carrera => carrera.Activa || carrera.Id == modelo.CarreraId)
            .Select(carrera => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = carrera.Id.ToString(),
                Text = carrera.Activa ? $"{carrera.Codigo} · {carrera.Nombre}" : $"{carrera.Codigo} · {carrera.Nombre} (inactiva)",
                Selected = carrera.Id == modelo.CarreraId
            }).ToList();
        modelo.Docentes = docentes
            .Where(docente => docente.Activo || docente.Id == modelo.DocenteId)
            .Select(docente => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = docente.Id.ToString(),
                Text = docente.Activo ? docente.Nombre : $"{docente.Nombre} (inactivo)",
                Selected = docente.Id == modelo.DocenteId
            }).ToList();
        return modelo;
    }

    private void PrepararCarrera(string titulo, bool edicion)
    {
        ViewData["Title"] = titulo;
        ViewData["Nav"] = "carreras";
        ViewData["Miga"] = edicion ? "Carreras y cursos / Editar carrera" : "Carreras y cursos / Nueva carrera";
    }

    private void PrepararCurso(string titulo, bool edicion)
    {
        ViewData["Title"] = titulo;
        ViewData["Nav"] = "carreras";
        ViewData["Miga"] = edicion ? "Carreras y cursos / Curso" : "Carreras y cursos / Nuevo curso";
        ViewData["Edicion"] = edicion;
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

    private IActionResult Vista(string nombre, string nav)
    {
        ViewData["Title"] = nombre;
        ViewData["Nav"] = nav;
        ViewData["Miga"] = $"Panel / {nombre}";
        return View("Index", nombre);
    }
}

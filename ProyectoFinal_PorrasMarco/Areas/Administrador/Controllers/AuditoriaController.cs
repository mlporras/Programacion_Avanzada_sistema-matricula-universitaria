using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_PorrasMarco.Data;
using ProyectoFinal_PorrasMarco.Models;
using ProyectoFinal_PorrasMarco.ViewModels;

namespace ProyectoFinal_PorrasMarco.Areas.Administrador.Controllers;

[Area("Administrador")]
[Authorize(Roles = RolesSistema.Administrador)]
public class AuditoriaController : Controller
{
    private readonly ApplicationDbContext _db;

    public AuditoriaController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("/administrador/auditoria")]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Auditoría";
        ViewData["Nav"] = "auditoria";
        ViewData["Miga"] = "Panel / Auditoría";
        var movimientos = await _db.MovimientosMatricula.AsNoTracking()
            .OrderByDescending(movimiento => movimiento.Fecha)
            .Select(movimiento => new MovimientoFilaVm
            {
                Fecha = movimiento.Fecha,
                Accion = movimiento.Accion,
                Curso = movimiento.Curso.Codigo + " · " + movimiento.Curso.Nombre,
                Estudiante = movimiento.Estudiante.Nombre,
                Responsable = movimiento.UsuarioResponsableId,
                Justificacion = movimiento.Justificacion,
                EstadoAnterior = movimiento.EstadoAnterior,
                EstadoNuevo = movimiento.EstadoNuevo
            })
            .ToListAsync();
        var usuarios = await _db.Users.AsNoTracking().ToDictionaryAsync(usuario => usuario.Id, usuario => usuario.NombreCompleto);
        foreach (var movimiento in movimientos)
        {
            if (usuarios.TryGetValue(movimiento.Responsable, out var nombre))
            {
                movimiento.Responsable = nombre;
            }
        }

        return View(movimientos);
    }
}

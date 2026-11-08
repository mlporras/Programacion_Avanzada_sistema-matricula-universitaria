using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_PorrasMarco.Data;
using ProyectoFinal_PorrasMarco.Models;

namespace ProyectoFinal_PorrasMarco.Filters;

public class NavegacionDocenteFilter : IAsyncActionFilter
{
    private readonly ApplicationDbContext _db;

    public NavegacionDocenteFilter(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var usuario = context.HttpContext.User;
        if (usuario.Identity?.IsAuthenticated == true && usuario.IsInRole(RolesSistema.Docente))
        {
            var id = usuario.FindFirstValue(ClaimTypes.NameIdentifier);
            var tieneCursos = await _db.Cursos.AnyAsync(curso =>
                curso.Activo && curso.Docente != null && curso.Docente.UsuarioId == id);
            context.HttpContext.Items["DocenteConCursos"] = tieneCursos;
        }

        await next();
    }
}

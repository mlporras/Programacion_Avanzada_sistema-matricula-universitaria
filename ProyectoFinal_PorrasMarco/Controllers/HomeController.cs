using Microsoft.AspNetCore.Mvc;
using ProyectoFinal_PorrasMarco.Models;

namespace ProyectoFinal_PorrasMarco.Controllers;

public class HomeController : Controller
{
    [HttpGet("/")]
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Redirect("/ingresar");
        }

        if (User.IsInRole(RolesSistema.Administrador))
        {
            return Redirect("/administrador");
        }

        if (User.IsInRole(RolesSistema.Docente))
        {
            return Redirect("/docente/cursos");
        }

        return Redirect("/estudiante");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new Models.ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

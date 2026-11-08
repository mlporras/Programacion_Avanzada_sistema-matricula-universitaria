using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ProyectoFinal_PorrasMarco.Servicios;

namespace ProyectoFinal_PorrasMarco.Controllers;

public abstract class ControladorAplicacion : Controller
{
    protected string UsuarioId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    protected void CopiarErrores(ResultadoOperacion resultado)
    {
        if (resultado.Errores.Count == 0)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return;
        }

        foreach (var error in resultado.Errores)
        {
            ModelState.AddModelError(error.Campo, error.Mensaje);
        }
    }
}

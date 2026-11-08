using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProyectoFinal_PorrasMarco.Models;
using ProyectoFinal_PorrasMarco.Servicios;
using ProyectoFinal_PorrasMarco.ViewModels;

namespace ProyectoFinal_PorrasMarco.Controllers;

public class CuentaController : ControladorAplicacion
{
    private readonly SignInManager<ApplicationUser> _sesion;
    private readonly UserManager<ApplicationUser> _usuarios;
    private readonly UsuarioServicio _servicio;

    public CuentaController(
        SignInManager<ApplicationUser> sesion,
        UserManager<ApplicationUser> usuarios,
        UsuarioServicio servicio)
    {
        _sesion = sesion;
        _usuarios = usuarios;
        _servicio = servicio;
    }

    [HttpGet("/ingresar")]
    public IActionResult Ingresar(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirigirAlPanel();
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new IngresoVm());
    }

    [HttpPost("/ingresar")]
    public async Task<IActionResult> Ingresar(IngresoVm modelo, string? returnUrl)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var correo = modelo.Correo.Trim();
        var resultado = await _sesion.PasswordSignInAsync(correo, modelo.Contrasena, isPersistent: false, lockoutOnFailure: true);
        if (resultado.Succeeded)
        {
            var usuario = await _usuarios.FindByEmailAsync(correo);
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirigirAlPanel(usuario);
        }

        var usuarioExistente = await _usuarios.FindByEmailAsync(correo);
        var bloqueoLargo = usuarioExistente?.LockoutEnd > DateTimeOffset.UtcNow.AddYears(1);
        var mensaje = resultado.IsLockedOut && bloqueoLargo
            ? "La cuenta no está activa. Solicite apoyo a la administración."
            : resultado.IsLockedOut
                ? "La cuenta está bloqueada temporalmente. Intente más tarde."
                : "Las credenciales no son válidas. Revise el correo y la contraseña.";

        ModelState.AddModelError(string.Empty, mensaje);
        if (!resultado.IsLockedOut)
        {
            ModelState.AddModelError(nameof(modelo.Correo), "Correo o contraseña incorrectos.");
        }

        return View(modelo);
    }

    [HttpGet("/registro")]
    public IActionResult Registro()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirigirAlPanel();
        }

        return View(new RegistroEstudianteVm());
    }

    [HttpPost("/registro")]
    public async Task<IActionResult> Registro(RegistroEstudianteVm modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var resultado = await _servicio.RegistrarEstudianteAsync(modelo);
        if (!resultado.Exito)
        {
            CopiarErrores(resultado);
            return View(modelo);
        }

        await _sesion.PasswordSignInAsync(modelo.Correo.Trim(), modelo.Contrasena, isPersistent: false, lockoutOnFailure: false);
        TempData["Mensaje"] = "Cuenta de estudiante creada";
        return Redirect("/sesion");
    }

    [Authorize]
    [HttpPost("/salir")]
    public async Task<IActionResult> Salir()
    {
        await _sesion.SignOutAsync();
        return Redirect("/ingresar");
    }

    [HttpGet("/acceso-denegado")]
    public IActionResult AccesoDenegado()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }

    private IActionResult RedirigirAlPanel(ApplicationUser? usuario = null)
    {
        if (usuario is not null)
        {
            if (_usuarios.IsInRoleAsync(usuario, RolesSistema.Administrador).GetAwaiter().GetResult())
            {
                return Redirect("/administrador");
            }

            return Redirect("/sesion");
        }

        if (User.IsInRole(RolesSistema.Administrador))
        {
            return Redirect("/administrador");
        }

        return Redirect("/sesion");
    }
}

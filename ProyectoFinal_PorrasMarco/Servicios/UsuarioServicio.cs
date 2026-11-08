using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_PorrasMarco.Data;
using ProyectoFinal_PorrasMarco.Models;
using ProyectoFinal_PorrasMarco.ViewModels;

namespace ProyectoFinal_PorrasMarco.Servicios;

public class UsuarioServicio
{
    private readonly UserManager<ApplicationUser> _usuarios;
    private readonly ApplicationDbContext _db;

    public UsuarioServicio(UserManager<ApplicationUser> usuarios, ApplicationDbContext db)
    {
        _usuarios = usuarios;
        _db = db;
    }

    public async Task<ResultadoOperacion> RegistrarEstudianteAsync(RegistroEstudianteVm modelo)
    {
        return await CrearCuentaAsync(modelo.Nombre, modelo.Correo, modelo.Contrasena, RolesSistema.Estudiante, activo: true);
    }

    public async Task<ResultadoOperacion> CrearPorAdministradorAsync(NuevoUsuarioVm modelo)
    {
        if (!RolesSistema.EsValido(modelo.Rol))
        {
            return ResultadoOperacion.Fallo("Seleccione un rol válido.", nameof(modelo.Rol));
        }

        return await CrearCuentaAsync(modelo.Nombre, modelo.Correo, modelo.Contrasena, modelo.Rol, modelo.Activo);
    }

    private async Task<ResultadoOperacion> CrearCuentaAsync(string nombre, string correo, string contrasena, string rol, bool activo)
    {
        var correoNormalizado = correo.Trim();
        if (await _usuarios.FindByEmailAsync(correoNormalizado) is not null)
        {
            return ResultadoOperacion.Fallo("El correo ya está registrado.", "Correo");
        }

        await using var transaccion = await _db.Database.BeginTransactionAsync();
        var usuario = new ApplicationUser
        {
            UserName = correoNormalizado,
            Email = correoNormalizado,
            EmailConfirmed = true,
            NombreCompleto = nombre.Trim(),
            LockoutEnabled = true
        };

        var creado = await _usuarios.CreateAsync(usuario, contrasena);
        if (!creado.Succeeded)
        {
            return ErroresDeIdentidad(creado);
        }

        var rolAsignado = await _usuarios.AddToRoleAsync(usuario, rol);
        if (!rolAsignado.Succeeded)
        {
            return ErroresDeIdentidad(rolAsignado);
        }

        if (!activo)
        {
            await _usuarios.SetLockoutEndDateAsync(usuario, DateTimeOffset.MaxValue);
        }

        if (rol == RolesSistema.Estudiante)
        {
            _db.Estudiantes.Add(new Estudiante
            {
                UsuarioId = usuario.Id,
                Nombre = usuario.NombreCompleto,
                Activo = activo
            });
        }
        else if (rol == RolesSistema.Docente)
        {
            _db.Docentes.Add(new Docente
            {
                UsuarioId = usuario.Id,
                Nombre = usuario.NombreCompleto,
                Activo = activo
            });
        }

        await _db.SaveChangesAsync();
        await transaccion.CommitAsync();
        return ResultadoOperacion.Ok("Usuario creado", id: null, detalle: usuario.Id);
    }

    private static ResultadoOperacion ErroresDeIdentidad(IdentityResult resultado)
    {
        var errores = resultado.Errors.Select(error =>
        {
            var campo = error.Code.Contains("Password", StringComparison.OrdinalIgnoreCase)
                ? "Contrasena"
                : error.Code.Contains("Email", StringComparison.OrdinalIgnoreCase) || error.Code.Contains("UserName", StringComparison.OrdinalIgnoreCase)
                    ? "Correo"
                    : string.Empty;
            return new ErrorCampo(campo, error.Description);
        }).ToList();

        return ResultadoOperacion.Fallo(errores, errores.FirstOrDefault()?.Mensaje ?? "No se creó la cuenta.");
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_PorrasMarco.Models;

namespace ProyectoFinal_PorrasMarco.Data;

public static class InicializadorBd
{
    public static async Task EjecutarAsync(IServiceProvider servicios)
    {
        var roles = servicios.GetRequiredService<RoleManager<IdentityRole>>();
        var usuarios = servicios.GetRequiredService<UserManager<ApplicationUser>>();
        var db = servicios.GetRequiredService<ApplicationDbContext>();
        var config = servicios.GetRequiredService<IConfiguration>();
        var logger = servicios.GetRequiredService<ILoggerFactory>().CreateLogger("InicializadorBd");

        foreach (var rol in RolesSistema.Todos)
        {
            if (!await roles.RoleExistsAsync(rol))
            {
                await roles.CreateAsync(new IdentityRole(rol));
            }
        }

        await AsegurarUsuarioAsync(
            usuarios,
            db,
            logger,
            config["Seed:AdminEmail"] ?? CorreosDemostracion.Admin,
            config["Seed:AdminPassword"],
            "Administración del sistema (demostración)",
            RolesSistema.Administrador,
            activo: true);

        if (!config.GetValue("Seed:CargarDemostracion", true))
        {
            return;
        }

        await AsegurarCarreraAsync(db, DatosDemostracion.CarreraSoftware, "Ingeniería de software (demostración)", true);
        await AsegurarCarreraAsync(db, DatosDemostracion.CarreraAdministracion, "Administración (demostración)", true);
        await AsegurarCarreraAsync(db, DatosDemostracion.CarreraInactiva, "Carrera inactiva (demostración)", false);
        await db.SaveChangesAsync();
    }

    private static async Task AsegurarCarreraAsync(ApplicationDbContext db, string codigo, string nombre, bool activa)
    {
        if (await db.Carreras.AnyAsync(carrera => carrera.Codigo == codigo))
        {
            return;
        }

        db.Carreras.Add(new Carrera
        {
            Codigo = codigo,
            Nombre = nombre,
            Activa = activa,
            EsDatoDemostracion = true
        });
    }

    private static async Task<ApplicationUser?> AsegurarUsuarioAsync(
        UserManager<ApplicationUser> usuarios,
        ApplicationDbContext db,
        ILogger logger,
        string? correo,
        string? contrasena,
        string nombre,
        string rol,
        bool activo)
    {
        if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(contrasena))
        {
            logger.LogWarning("No se creó {Correo} porque falta la contraseña en la configuración local.", correo);
            return null;
        }

        var existente = await usuarios.FindByEmailAsync(correo);
        if (existente is not null)
        {
            await AsegurarPerfilAsync(db, existente, nombre, rol, activo);
            return existente;
        }

        var usuario = new ApplicationUser
        {
            UserName = correo,
            Email = correo,
            EmailConfirmed = true,
            NombreCompleto = nombre,
            LockoutEnabled = true
        };

        var creado = await usuarios.CreateAsync(usuario, contrasena);
        if (!creado.Succeeded)
        {
            logger.LogError("No se pudo crear {Correo}: {Errores}", correo, string.Join(" ", creado.Errors.Select(error => error.Description)));
            return null;
        }

        await usuarios.AddToRoleAsync(usuario, rol);
        if (!activo)
        {
            await usuarios.SetLockoutEndDateAsync(usuario, DateTimeOffset.MaxValue);
        }

        await AsegurarPerfilAsync(db, usuario, nombre, rol, activo);
        return usuario;
    }

    private static async Task AsegurarPerfilAsync(ApplicationDbContext db, ApplicationUser usuario, string nombre, string rol, bool activo)
    {
        if (rol == RolesSistema.Estudiante && !await db.Estudiantes.AnyAsync(estudiante => estudiante.UsuarioId == usuario.Id))
        {
            db.Estudiantes.Add(new Estudiante
            {
                UsuarioId = usuario.Id,
                Nombre = nombre,
                Activo = activo,
                EsDatoDemostracion = true
            });
            await db.SaveChangesAsync();
        }

        if (rol == RolesSistema.Docente && !await db.Docentes.AnyAsync(docente => docente.UsuarioId == usuario.Id))
        {
            db.Docentes.Add(new Docente
            {
                UsuarioId = usuario.Id,
                Nombre = nombre,
                Activo = activo,
                EsDatoDemostracion = true
            });
            await db.SaveChangesAsync();
        }
    }
}

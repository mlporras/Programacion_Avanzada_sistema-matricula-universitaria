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

        await AsegurarCatalogoAsync(db);

        var contrasenaDemo = config["Seed:ContrasenaDemostracion"];
        if (string.IsNullOrWhiteSpace(contrasenaDemo))
        {
            logger.LogWarning("Falta Seed:ContrasenaDemostracion. El catálogo se cargó sin usuarios de demostración.");
            return;
        }

        var docente = await AsegurarUsuarioAsync(usuarios, db, logger, config["Seed:DocenteEmail"] ?? CorreosDemostracion.Docente, contrasenaDemo, "Carmen Rojas (demostración)", RolesSistema.Docente, true);
        await AsegurarUsuarioAsync(usuarios, db, logger, config["Seed:DocenteSinCursosEmail"] ?? CorreosDemostracion.DocenteSinCursos, contrasenaDemo, "Pedro Quesada (demostración)", RolesSistema.Docente, true);
        await AsegurarUsuarioAsync(usuarios, db, logger, config["Seed:DocenteInactivoEmail"] ?? CorreosDemostracion.DocenteInactivo, contrasenaDemo, "Elena Vargas (demostración)", RolesSistema.Docente, false);
        var estudiante = await AsegurarUsuarioAsync(usuarios, db, logger, config["Seed:EstudianteEmail"] ?? CorreosDemostracion.Estudiante, contrasenaDemo, "Ana Solís (demostración)", RolesSistema.Estudiante, true);
        var estudianteSinMatricula = await AsegurarUsuarioAsync(usuarios, db, logger, config["Seed:EstudianteSinMatriculaEmail"] ?? CorreosDemostracion.EstudianteSinMatricula, contrasenaDemo, "Luis Mora (demostración)", RolesSistema.Estudiante, true);

        if (docente is null || estudiante is null || estudianteSinMatricula is null)
        {
            return;
        }

        await VincularDemostracionAsync(db, docente, estudiante, estudianteSinMatricula);
    }

    private static async Task AsegurarCatalogoAsync(ApplicationDbContext db)
    {
        await AsegurarCarreraAsync(db, DatosDemostracion.CarreraSoftware, "Ingeniería de software (demostración)", true);
        await AsegurarCarreraAsync(db, DatosDemostracion.CarreraAdministracion, "Administración (demostración)", true);
        await AsegurarCarreraAsync(db, DatosDemostracion.CarreraInactiva, "Carrera inactiva (demostración)", false);
        await db.SaveChangesAsync();

        var software = await db.Carreras.SingleAsync(carrera => carrera.Codigo == DatosDemostracion.CarreraSoftware);
        var administracion = await db.Carreras.SingleAsync(carrera => carrera.Codigo == DatosDemostracion.CarreraAdministracion);

        await AsegurarCursoAsync(db, DatosDemostracion.CursoProgramacion, "Programación (demostración)", 4, software.Id, DatosDemostracion.PeriodoConNotas, true);
        await AsegurarCursoAsync(db, DatosDemostracion.CursoBases, "Bases de datos (demostración)", 4, software.Id, DatosDemostracion.PeriodoConNotas, true);
        await AsegurarCursoAsync(db, DatosDemostracion.CursoRedes, "Redes (demostración)", 3, software.Id, DatosDemostracion.PeriodoSinNotas, true);
        await AsegurarCursoAsync(db, DatosDemostracion.CursoContabilidad, "Contabilidad (demostración)", 3, administracion.Id, DatosDemostracion.PeriodoConNotas, true);
        await AsegurarCursoAsync(db, DatosDemostracion.CursoInactivo, "Curso inactivo (demostración)", 2, software.Id, DatosDemostracion.PeriodoConNotas, false);
        await db.SaveChangesAsync();
    }

    private static async Task VincularDemostracionAsync(
        ApplicationDbContext db,
        ApplicationUser docenteUsuario,
        ApplicationUser estudianteUsuario,
        ApplicationUser estudianteSinMatriculaUsuario)
    {
        var docente = await db.Docentes.SingleAsync(item => item.UsuarioId == docenteUsuario.Id);
        var estudiante = await db.Estudiantes.SingleAsync(item => item.UsuarioId == estudianteUsuario.Id);
        var sinMatricula = await db.Estudiantes.SingleAsync(item => item.UsuarioId == estudianteSinMatriculaUsuario.Id);
        var software = await db.Carreras.SingleAsync(carrera => carrera.Codigo == DatosDemostracion.CarreraSoftware);

        if (estudiante.CarreraId is null)
        {
            estudiante.CarreraId = software.Id;
        }

        if (sinMatricula.CarreraId is null)
        {
            sinMatricula.CarreraId = software.Id;
        }

        var codigosAsignados = new[]
        {
            DatosDemostracion.CursoProgramacion,
            DatosDemostracion.CursoBases,
            DatosDemostracion.CursoRedes,
            DatosDemostracion.CursoContabilidad
        };

        var cursos = await db.Cursos.Where(curso => codigosAsignados.Contains(curso.Codigo)).ToListAsync();
        foreach (var curso in cursos.Where(curso => curso.DocenteId is null))
        {
            curso.DocenteId = docente.Id;
        }

        await db.SaveChangesAsync();

        var programacion = cursos.Single(curso => curso.Codigo == DatosDemostracion.CursoProgramacion);
        var bases = cursos.Single(curso => curso.Codigo == DatosDemostracion.CursoBases);
        await AsegurarMatriculaDemostracionAsync(db, estudiante, programacion, DatosDemostracion.NotaProgramacion);
        await AsegurarMatriculaDemostracionAsync(db, estudiante, bases, DatosDemostracion.NotaBases);
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

    private static async Task AsegurarCursoAsync(
        ApplicationDbContext db,
        string codigo,
        string nombre,
        int creditos,
        int carreraId,
        string periodo,
        bool activo)
    {
        if (await db.Cursos.AnyAsync(curso => curso.Codigo == codigo))
        {
            return;
        }

        db.Cursos.Add(new Curso
        {
            Codigo = codigo,
            Nombre = nombre,
            Creditos = creditos,
            CarreraId = carreraId,
            Periodo = periodo,
            Activo = activo,
            EsDatoDemostracion = true
        });
    }

    private static async Task AsegurarMatriculaDemostracionAsync(ApplicationDbContext db, Estudiante estudiante, Curso curso, decimal nota)
    {
        var existente = await db.Matriculas.Include(matricula => matricula.Calificacion)
            .FirstOrDefaultAsync(matricula => matricula.EstudianteId == estudiante.Id && matricula.CursoId == curso.Id);
        if (existente is null)
        {
            existente = new Matricula
            {
                EstudianteId = estudiante.Id,
                CursoId = curso.Id,
                Periodo = curso.Periodo,
                Estado = EstadoMatricula.Activa,
                FechaMatricula = new DateTime(2026, 3, 2, 9, 0, 0)
            };
            db.Matriculas.Add(existente);
            await db.SaveChangesAsync();
        }

        if (existente.Calificacion is not null)
        {
            if (existente.Calificacion.EscalaReferencia != DatosDemostracion.Escala)
            {
                existente.Calificacion.EscalaReferencia = DatosDemostracion.Escala;
                await db.SaveChangesAsync();
            }

            return;
        }

        db.Calificaciones.Add(new Calificacion
        {
            MatriculaId = existente.Id,
            Valor = nota,
            EsDatoDemostracion = true,
            FechaRegistro = new DateTime(2026, 6, 15, 10, 0, 0),
            EscalaReferencia = DatosDemostracion.Escala
        });
        await db.SaveChangesAsync();
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

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_PorrasMarco.Models;

namespace ProyectoFinal_PorrasMarco.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Carrera> Carreras => Set<Carrera>();

    public DbSet<Curso> Cursos => Set<Curso>();

    public DbSet<Docente> Docentes => Set<Docente>();

    public DbSet<Estudiante> Estudiantes => Set<Estudiante>();

    public DbSet<Matricula> Matriculas => Set<Matricula>();

    public DbSet<Calificacion> Calificaciones => Set<Calificacion>();

    public DbSet<MovimientoMatricula> MovimientosMatricula => Set<MovimientoMatricula>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>()
            .Property(usuario => usuario.NombreCompleto)
            .HasMaxLength(120)
            .IsRequired();

        builder.Entity<Carrera>(entidad =>
        {
            entidad.Property(carrera => carrera.Codigo).HasMaxLength(20).IsRequired();
            entidad.Property(carrera => carrera.Nombre).HasMaxLength(150).IsRequired();
            entidad.HasIndex(carrera => carrera.Codigo).IsUnique();
        });

        builder.Entity<Curso>(entidad =>
        {
            entidad.Property(curso => curso.Codigo).HasMaxLength(20).IsRequired();
            entidad.Property(curso => curso.Nombre).HasMaxLength(150).IsRequired();
            entidad.Property(curso => curso.Periodo).HasMaxLength(40).IsRequired();
            entidad.HasIndex(curso => curso.Codigo).IsUnique();
            entidad.HasOne(curso => curso.Carrera)
                .WithMany(carrera => carrera.Cursos)
                .HasForeignKey(curso => curso.CarreraId)
                .OnDelete(DeleteBehavior.Restrict);
            entidad.HasOne(curso => curso.Docente)
                .WithMany(docente => docente.Cursos)
                .HasForeignKey(curso => curso.DocenteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Docente>(entidad =>
        {
            entidad.Property(docente => docente.Nombre).HasMaxLength(120).IsRequired();
            entidad.Property(docente => docente.UsuarioId).HasMaxLength(450).IsRequired();
            entidad.HasIndex(docente => docente.UsuarioId).IsUnique();
            entidad.HasOne(docente => docente.Usuario)
                .WithMany()
                .HasForeignKey(docente => docente.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Estudiante>(entidad =>
        {
            entidad.Property(estudiante => estudiante.Nombre).HasMaxLength(120).IsRequired();
            entidad.Property(estudiante => estudiante.UsuarioId).HasMaxLength(450).IsRequired();
            entidad.HasIndex(estudiante => estudiante.UsuarioId).IsUnique();
            entidad.HasOne(estudiante => estudiante.Usuario)
                .WithMany()
                .HasForeignKey(estudiante => estudiante.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
            entidad.HasOne(estudiante => estudiante.Carrera)
                .WithMany(carrera => carrera.Estudiantes)
                .HasForeignKey(estudiante => estudiante.CarreraId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Matricula>(entidad =>
        {
            entidad.Property(matricula => matricula.Periodo).HasMaxLength(40).IsRequired();
            entidad.Property(matricula => matricula.Estado).HasConversion<int>();
            entidad.HasIndex(matricula => new { matricula.EstudianteId, matricula.CursoId })
                .IsUnique()
                .HasFilter("[Estado] = 1");
            entidad.HasOne(matricula => matricula.Estudiante)
                .WithMany(estudiante => estudiante.Matriculas)
                .HasForeignKey(matricula => matricula.EstudianteId)
                .OnDelete(DeleteBehavior.Restrict);
            entidad.HasOne(matricula => matricula.Curso)
                .WithMany(curso => curso.Matriculas)
                .HasForeignKey(matricula => matricula.CursoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Calificacion>(entidad =>
        {
            entidad.Property(calificacion => calificacion.Valor).HasPrecision(5, 2);
            entidad.Property(calificacion => calificacion.EscalaReferencia).HasMaxLength(80).IsRequired();
            entidad.HasIndex(calificacion => calificacion.MatriculaId).IsUnique();
            entidad.HasOne(calificacion => calificacion.Matricula)
                .WithOne(matricula => matricula.Calificacion)
                .HasForeignKey<Calificacion>(calificacion => calificacion.MatriculaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<MovimientoMatricula>(entidad =>
        {
            entidad.Property(movimiento => movimiento.UsuarioResponsableId).HasMaxLength(450).IsRequired();
            entidad.Property(movimiento => movimiento.Accion).HasMaxLength(20).IsRequired();
            entidad.Property(movimiento => movimiento.EstadoAnterior).HasMaxLength(20).IsRequired();
            entidad.Property(movimiento => movimiento.EstadoNuevo).HasMaxLength(20).IsRequired();
            entidad.Property(movimiento => movimiento.Justificacion).HasMaxLength(500).IsRequired();
            entidad.HasOne(movimiento => movimiento.Curso)
                .WithMany()
                .HasForeignKey(movimiento => movimiento.CursoId)
                .OnDelete(DeleteBehavior.Restrict);
            entidad.HasOne(movimiento => movimiento.Estudiante)
                .WithMany()
                .HasForeignKey(movimiento => movimiento.EstudianteId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

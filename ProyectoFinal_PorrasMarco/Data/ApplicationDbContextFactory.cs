using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProyectoFinal_PorrasMarco.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public const string ConexionLocal =
        "Server=localhost\\SQLEXPRESS;Database=MatriculaUniversitaria;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var opciones = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(ConexionLocal)
            .Options;
        return new ApplicationDbContext(opciones);
    }
}

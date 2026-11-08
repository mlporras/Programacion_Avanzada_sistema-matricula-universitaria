namespace ProyectoFinal_PorrasMarco.Models;

public static class RolesSistema
{
    public const string Administrador = "Administrador";
    public const string Docente = "Docente";
    public const string Estudiante = "Estudiante";

    public static readonly string[] Todos = [Administrador, Docente, Estudiante];

    public static bool EsValido(string? rol) =>
        rol is Administrador or Docente or Estudiante;
}

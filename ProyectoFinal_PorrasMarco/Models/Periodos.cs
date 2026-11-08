namespace ProyectoFinal_PorrasMarco.Models;

public static class Periodos
{
    public const string Vigente = DatosDemostracion.PeriodoConNotas;

    private static readonly (string Codigo, string Etiqueta)[] Conocidos =
    [
        (DatosDemostracion.PeriodoConNotas, "I semestre 2026"),
        (DatosDemostracion.PeriodoSinNotas, "II semestre 2026")
    ];

    public static string Etiqueta(string? codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            return string.Empty;
        }

        var texto = codigo.Trim();
        foreach (var periodo in Conocidos)
        {
            if (periodo.Codigo.Equals(texto, StringComparison.OrdinalIgnoreCase)
                || periodo.Etiqueta.Equals(texto, StringComparison.OrdinalIgnoreCase))
            {
                return periodo.Etiqueta;
            }
        }

        return texto;
    }

    public static string Normalizar(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return Vigente;
        }

        var texto = valor.Trim();
        foreach (var periodo in Conocidos)
        {
            if (periodo.Codigo.Equals(texto, StringComparison.OrdinalIgnoreCase)
                || periodo.Etiqueta.Equals(texto, StringComparison.OrdinalIgnoreCase))
            {
                return periodo.Codigo;
            }
        }

        return texto;
    }

    public static IReadOnlyList<string> Equivalentes(string codigo)
    {
        var normalizado = Normalizar(codigo);
        var etiqueta = Etiqueta(normalizado);
        return normalizado == etiqueta ? [normalizado] : [normalizado, etiqueta];
    }

    public static IReadOnlyList<(string Codigo, string Etiqueta)> Opciones(IEnumerable<string> adicionales)
    {
        var codigos = Conocidos.Select(periodo => periodo.Codigo).ToList();
        foreach (var adicional in adicionales)
        {
            var codigo = Normalizar(adicional);
            if (!codigos.Contains(codigo, StringComparer.OrdinalIgnoreCase))
            {
                codigos.Add(codigo);
            }
        }

        return codigos.Select(codigo => (codigo, Etiqueta(codigo))).ToList();
    }
}

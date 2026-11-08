namespace ProyectoFinal_PorrasMarco.Servicios;

public sealed record ErrorCampo(string Campo, string Mensaje);

public sealed class ResultadoOperacion
{
    public bool Exito { get; init; }

    public string Mensaje { get; init; } = string.Empty;

    public string? Detalle { get; init; }

    public int? Id { get; init; }

    public IReadOnlyList<ErrorCampo> Errores { get; init; } = [];

    public static ResultadoOperacion Ok(string mensaje, int? id = null, string? detalle = null) =>
        new() { Exito = true, Mensaje = mensaje, Id = id, Detalle = detalle };

    public static ResultadoOperacion Fallo(string mensaje, string campo = "", string? detalle = null)
    {
        var errores = string.IsNullOrEmpty(campo)
            ? Array.Empty<ErrorCampo>()
            : [new ErrorCampo(campo, mensaje)];
        return new ResultadoOperacion
        {
            Exito = false,
            Mensaje = mensaje,
            Detalle = detalle,
            Errores = errores
        };
    }

    public static ResultadoOperacion Fallo(IReadOnlyList<ErrorCampo> errores, string mensaje) =>
        new() { Exito = false, Mensaje = mensaje, Errores = errores };
}

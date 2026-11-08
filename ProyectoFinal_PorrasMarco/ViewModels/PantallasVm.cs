namespace ProyectoFinal_PorrasMarco.ViewModels;

public class UsuarioFilaVm
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

public class UsuarioCreadoVm
{
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string? Mensaje { get; set; }
    public string? Detalle { get; set; }
}

public class PanelAdministradorVm
{
    public int Usuarios { get; set; }
    public int CursosActivos { get; set; }
    public int MatriculasActivas { get; set; }
}

public class CarreraFilaVm
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activa { get; set; }
    public bool EsDemostracion { get; set; }
}

public class CursoFilaVm
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Carrera { get; set; } = string.Empty;
    public string Periodo { get; set; } = string.Empty;
    public string Docente { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public bool EsDemostracion { get; set; }
}

public class OfertaAdministradorVm
{
    public int? CarreraId { get; set; }
    public string? Periodo { get; set; }
    public List<CarreraFilaVm> Carreras { get; set; } = [];
    public List<CursoFilaVm> Cursos { get; set; } = [];
    public List<string> Periodos { get; set; } = [];
}

public class ConfirmacionCarreraVm
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string? Mensaje { get; set; }
}

public class ConfirmacionCursoVm
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Carrera { get; set; } = string.Empty;
    public string Periodo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Docente { get; set; } = string.Empty;
    public string? Mensaje { get; set; }
    public string? Detalle { get; set; }
}

public class PanelEstudianteVm
{
    public string Carrera { get; set; } = "Sin carrera seleccionada";
    public int CursosActivos { get; set; }
}

public class CursoOfertaFilaVm
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Periodo { get; set; } = string.Empty;
    public string Docente { get; set; } = "Sin docente asignado";
    public bool YaMatriculado { get; set; }
}

public class PeriodoOpcionVm
{
    public string Codigo { get; set; } = string.Empty;
    public string Etiqueta { get; set; } = string.Empty;
}

public class OfertaEstudianteVm
{
    public bool SinCarrera { get; set; }
    public string Carrera { get; set; } = string.Empty;
    public string Periodo { get; set; } = string.Empty;
    public string PeriodoEtiqueta { get; set; } = string.Empty;
    public List<PeriodoOpcionVm> Periodos { get; set; } = [];
    public List<CursoOfertaFilaVm> Cursos { get; set; } = [];
}

public class DetalleCursoDisponibleVm
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Periodo { get; set; } = string.Empty;
    public string Docente { get; set; } = string.Empty;
    public string? Creditos { get; set; }
    public bool YaMatriculado { get; set; }
    public string? Error { get; set; }
    public string PeriodoConsulta { get; set; } = string.Empty;
}

public class CursoMatriculadoFilaVm
{
    public int MatriculaId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Periodo { get; set; } = string.Empty;
    public string Docente { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}

public class MisCursosVm
{
    public List<CursoMatriculadoFilaVm> Activos { get; set; } = [];
    public List<CursoMatriculadoFilaVm> Retiros { get; set; } = [];
}

public class DetalleMatriculaVm
{
    public int MatriculaId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Carrera { get; set; } = string.Empty;
    public string Periodo { get; set; } = string.Empty;
    public string Docente { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string? Creditos { get; set; }
    public string? Mensaje { get; set; }
    public string? Detalle { get; set; }
}

public class FilaReporteVm
{
    public string Curso { get; set; } = string.Empty;
    public decimal? Valor { get; set; }
    public bool EsDemostracion { get; set; }
    public string Estado { get; set; } = string.Empty;
}

public class ReportePersonalVm
{
    public string Periodo { get; set; } = string.Empty;
    public List<string> Periodos { get; set; } = [];
    public bool Vacio { get; set; }
    public string Escala { get; set; } = string.Empty;
    public List<FilaReporteVm> Filas { get; set; } = [];
}

public class CursoDocenteFilaVm
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Periodo { get; set; } = string.Empty;
    public int Estudiantes { get; set; }
}

public class DetalleCursoDocenteVm
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Carrera { get; set; } = string.Empty;
    public string Periodo { get; set; } = string.Empty;
    public int Estudiantes { get; set; }
    public string Estado { get; set; } = string.Empty;
}

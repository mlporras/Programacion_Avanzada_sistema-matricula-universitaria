using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProyectoFinal_PorrasMarco.Models;

namespace ProyectoFinal_PorrasMarco.ViewModels;

public class IngresoVm
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    public string Contrasena { get; set; } = string.Empty;
}

public class RegistroEstudianteVm
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120, ErrorMessage = "El nombre admite hasta 120 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [DataType(DataType.Password)]
    public string Contrasena { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme la contraseña.")]
    [Compare(nameof(Contrasena), ErrorMessage = "Las contraseñas no coinciden.")]
    [DataType(DataType.Password)]
    public string ConfirmarContrasena { get; set; } = string.Empty;
}

public class NuevoUsuarioVm
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120, ErrorMessage = "El nombre admite hasta 120 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [DataType(DataType.Password)]
    public string Contrasena { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme la contraseña.")]
    [Compare(nameof(Contrasena), ErrorMessage = "Las contraseñas no coinciden.")]
    [DataType(DataType.Password)]
    public string ConfirmarContrasena { get; set; } = string.Empty;

    [Required(ErrorMessage = "Seleccione un rol.")]
    public string Rol { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}

public class CarreraFormVm
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(20, ErrorMessage = "El código admite hasta 20 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre admite hasta 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    public bool Activa { get; set; } = true;
}

public class CursoFormVm
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(20, ErrorMessage = "El código admite hasta 20 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre admite hasta 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Range(1, 30, ErrorMessage = "Los créditos deben estar entre 1 y 30.")]
    public int? Creditos { get; set; }

    [Required(ErrorMessage = "Seleccione una carrera.")]
    public int? CarreraId { get; set; }

    [Required(ErrorMessage = "El periodo es obligatorio.")]
    [StringLength(40, ErrorMessage = "El periodo admite hasta 40 caracteres.")]
    public string Periodo { get; set; } = DatosDemostracion.PeriodoConNotas;

    public bool Activo { get; set; } = true;

    public int? DocenteId { get; set; }

    public List<SelectListItem> Carreras { get; set; } = [];

    public List<SelectListItem> Docentes { get; set; } = [];
}

public class SeleccionarCarreraVm
{
    [Required(ErrorMessage = "Seleccione una carrera.")]
    public int? CarreraId { get; set; }

    public List<SelectListItem> Carreras { get; set; } = [];
}

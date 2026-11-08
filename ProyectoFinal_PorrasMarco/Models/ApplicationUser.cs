using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ProyectoFinal_PorrasMarco.Models;

public class ApplicationUser : IdentityUser
{
    [PersonalData]
    [MaxLength(120)]
    public string NombreCompleto { get; set; } = string.Empty;
}

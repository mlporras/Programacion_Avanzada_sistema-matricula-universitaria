using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ProyectoFinal_PorrasMarco.Models;

namespace ProyectoFinal_PorrasMarco.Identity;

public class FabricaClaims : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    public FabricaClaims(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identidad = await base.GenerateClaimsAsync(user);
        identidad.AddClaim(new Claim("nombre_completo", user.NombreCompleto));
        return identidad;
    }
}

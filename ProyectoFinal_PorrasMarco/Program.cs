using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_PorrasMarco.Data;
using ProyectoFinal_PorrasMarco.Filters;
using ProyectoFinal_PorrasMarco.Identity;
using ProyectoFinal_PorrasMarco.Models;
using ProyectoFinal_PorrasMarco.Servicios;

var builder = WebApplication.CreateBuilder(args);

var cultura = new CultureInfo("es-CR");
CultureInfo.DefaultThreadCurrentCulture = cultura;
CultureInfo.DefaultThreadCurrentUICulture = cultura;

builder.Services.AddDbContext<ApplicationDbContext>(opciones =>
    opciones.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(opciones =>
    {
        opciones.User.RequireUniqueEmail = true;
        opciones.SignIn.RequireConfirmedAccount = false;
        opciones.Password.RequiredLength = 8;
        opciones.Password.RequireDigit = true;
        opciones.Password.RequireLowercase = true;
        opciones.Password.RequireUppercase = true;
        opciones.Password.RequireNonAlphanumeric = true;
        opciones.Lockout.MaxFailedAccessAttempts = 5;
        opciones.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddErrorDescriber<DescripcionErroresIdentidad>()
    .AddClaimsPrincipalFactory<FabricaClaims>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(opciones =>
{
    opciones.LoginPath = "/ingresar";
    opciones.AccessDeniedPath = "/acceso-denegado";
    opciones.LogoutPath = "/salir";
    opciones.Cookie.Name = "Matricula.Auth";
    opciones.Cookie.HttpOnly = true;
    opciones.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    opciones.SlidingExpiration = true;
    opciones.ExpireTimeSpan = TimeSpan.FromHours(8);
});

builder.Services.AddScoped<UsuarioServicio>();
builder.Services.AddScoped<CarreraServicio>();
builder.Services.AddScoped<CursoServicio>();
builder.Services.AddScoped<OfertaServicio>();
builder.Services.AddScoped<MatriculaServicio>();
builder.Services.AddScoped<ReporteServicio>();
builder.Services.AddScoped<NavegacionDocenteFilter>();

builder.Services.AddControllersWithViews(opciones =>
{
    opciones.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
    opciones.Filters.Add(new Microsoft.AspNetCore.Mvc.ServiceFilterAttribute(typeof(NavegacionDocenteFilter)));
});

var app = builder.Build();

var localizacion = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultura),
    SupportedCultures = [cultura],
    SupportedUICultures = [cultura]
};
app.UseRequestLocalization(localizacion);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Panel}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

if (!app.Configuration.GetValue("Seed:OmitirInicioAutomatico", false))
{
    using var alcance = app.Services.CreateScope();
    var db = alcance.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    await InicializadorBd.EjecutarAsync(alcance.ServiceProvider);
}

app.Run();

public partial class Program { }

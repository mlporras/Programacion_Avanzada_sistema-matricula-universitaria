using Microsoft.EntityFrameworkCore;
using ProyectoFinal_PorrasMarco.Data;
using ProyectoFinal_PorrasMarco.Models;
using ProyectoFinal_PorrasMarco.ViewModels;

namespace ProyectoFinal_PorrasMarco.Servicios;

public class CarreraServicio
{
    private readonly ApplicationDbContext _db;

    public CarreraServicio(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ResultadoOperacion> GuardarAsync(CarreraFormVm modelo)
    {
        var codigo = modelo.Codigo.Trim().ToUpperInvariant();
        var nombre = modelo.Nombre.Trim();
        var id = modelo.Id ?? 0;

        if (await _db.Carreras.AnyAsync(carrera => carrera.Codigo == codigo && carrera.Id != id))
        {
            return ResultadoOperacion.Fallo("El código ya está registrado.", nameof(modelo.Codigo));
        }

        await using var transaccion = await _db.Database.BeginTransactionAsync();
        Carrera carrera;
        var esNueva = id == 0;
        if (esNueva)
        {
            carrera = new Carrera();
            _db.Carreras.Add(carrera);
        }
        else
        {
            var existente = await _db.Carreras.FirstOrDefaultAsync(item => item.Id == id);
            if (existente is null)
            {
                return ResultadoOperacion.Fallo("La carrera no existe.");
            }

            carrera = existente;
        }

        carrera.Codigo = codigo;
        carrera.Nombre = nombre;
        carrera.Activa = modelo.Activa;

        try
        {
            await _db.SaveChangesAsync();
            await transaccion.CommitAsync();
        }
        catch (DbUpdateException)
        {
            return ResultadoOperacion.Fallo("El código ya está registrado.", nameof(modelo.Codigo));
        }

        return ResultadoOperacion.Ok(esNueva ? "Carrera creada" : "Carrera actualizada", carrera.Id);
    }
}

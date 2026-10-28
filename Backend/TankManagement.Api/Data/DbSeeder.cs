using Microsoft.EntityFrameworkCore;
using TankManagement.Api.Models;

namespace TankManagement.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(TankManagementDbContext db)
    {
        if (!await db.Roles.AnyAsync())
        {
            db.Roles.AddRange(new Rol { Id = 1, Nombre = "Dueño" }, new Rol { Id = 2, Nombre = "Admin" });
            await db.SaveChangesAsync();
        }

        if (!await db.Usuarios.AnyAsync())
        {
            db.Usuarios.Add(new Usuario
            {
                Nombre = "Administrador",
                UsuarioNombre = "admin",
                Contrasena = "123456",
                RolId = 2
            });
            await db.SaveChangesAsync();
        }
    }
}

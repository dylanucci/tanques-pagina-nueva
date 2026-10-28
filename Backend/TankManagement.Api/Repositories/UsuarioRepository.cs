using Microsoft.EntityFrameworkCore;
using TankManagement.Api.Data;
using TankManagement.Api.Interfaces;
using TankManagement.Api.Models;
namespace TankManagement.Api.Repositories;
public class UsuarioRepository(TankManagementDbContext db) : IUsuarioRepository
{
    public Task<Usuario?> ObtenerPorIdAsync(int id) => db.Usuarios.Include(x => x.Rol).FirstOrDefaultAsync(x => x.Id == id);
    public Task<Usuario?> ObtenerPorUsuarioAsync(string usuario) => db.Usuarios.Include(x => x.Rol).FirstOrDefaultAsync(x => x.UsuarioNombre == usuario);
    public Task<List<Usuario>> ObtenerTodosAsync() => db.Usuarios.Include(x => x.Rol).OrderBy(x => x.Id).ToListAsync();
    public async Task CrearAsync(Usuario usuario) { db.Usuarios.Add(usuario); await db.SaveChangesAsync(); }
    public async Task EliminarAsync(int id) { var u = await db.Usuarios.FindAsync(id); if (u is null) return; db.Usuarios.Remove(u); await db.SaveChangesAsync(); }
}

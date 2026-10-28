using Microsoft.EntityFrameworkCore;
using TankManagement.Api.Data;
using TankManagement.Api.Interfaces;
using TankManagement.Api.Models;
namespace TankManagement.Api.Repositories;
public class TanqueRepository(TankManagementDbContext db) : ITanqueRepository
{
    private IQueryable<Tanque> Query() => db.Tanques.Include(x => x.UsuarioTanques).ThenInclude(x => x.Usuario).ThenInclude(x => x.Rol);
    public Task<Tanque?> ObtenerPorIdAsync(int id) => Query().FirstOrDefaultAsync(x => x.Id == id);
    public Task<List<Tanque>> ObtenerTodosAsync() => Query().OrderBy(x => x.Id).ToListAsync();
    public Task<List<Tanque>> ObtenerPorUsuarioIdAsync(int usuarioId) => Query().Where(x => x.UsuarioTanques.Any(ut => ut.UsuarioId == usuarioId)).ToListAsync();
    public async Task CrearAsync(Tanque tanque) { db.Tanques.Add(tanque); await db.SaveChangesAsync(); }
    public async Task EliminarAsync(int id) { var t = await db.Tanques.FindAsync(id); if (t is null) return; db.Tanques.Remove(t); await db.SaveChangesAsync(); }
    public async Task ActualizarCantidadAsync(int id, decimal cantidad) { var t = await db.Tanques.FindAsync(id) ?? throw new KeyNotFoundException("Tanque no encontrado."); t.CapacidadActualCM3 = cantidad; await db.SaveChangesAsync(); }
}

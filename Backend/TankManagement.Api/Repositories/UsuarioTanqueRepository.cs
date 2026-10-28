using Microsoft.EntityFrameworkCore;
using TankManagement.Api.Data;
using TankManagement.Api.Interfaces;
using TankManagement.Api.Models;
namespace TankManagement.Api.Repositories;
public class UsuarioTanqueRepository(TankManagementDbContext db) : IUsuarioTanqueRepository
{
    public async Task AsignarAsync(int usuarioId, int tanqueId)
    {
        if (!await db.UsuarioTanques.AnyAsync(x => x.UsuarioId == usuarioId && x.TanqueId == tanqueId))
        { db.UsuarioTanques.Add(new UsuarioTanque { UsuarioId = usuarioId, TanqueId = tanqueId }); await db.SaveChangesAsync(); }
    }
    public async Task DesasignarAsync(int usuarioId, int tanqueId)
    {
        var item = await db.UsuarioTanques.FirstOrDefaultAsync(x => x.UsuarioId == usuarioId && x.TanqueId == tanqueId);
        if (item is not null) { db.UsuarioTanques.Remove(item); await db.SaveChangesAsync(); }
    }
}

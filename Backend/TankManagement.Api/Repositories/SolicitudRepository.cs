using Microsoft.EntityFrameworkCore;
using TankManagement.Api.Data;
using TankManagement.Api.Interfaces;
using TankManagement.Api.Models;
namespace TankManagement.Api.Repositories;
public class SolicitudRepository(TankManagementDbContext db) : ISolicitudRepository
{
    public Task<List<Solicitud>> ObtenerPendientesAsync() => db.Solicitudes.Where(x => !x.Estado).OrderBy(x => x.Fecha).ThenBy(x => x.Hora).ThenBy(x => x.Id).ToListAsync();
    public Task<Solicitud?> ObtenerPorIdAsync(int id) => db.Solicitudes.FirstOrDefaultAsync(x => x.Id == id);
    public async Task CrearAsync(Solicitud solicitud) { db.Solicitudes.Add(solicitud); await db.SaveChangesAsync(); }
    public async Task ActualizarEstadoAsync(int id, bool estado)
    {
        var solicitud = await db.Solicitudes.FindAsync(id) ?? throw new KeyNotFoundException("Solicitud no encontrada.");
        solicitud.Estado = estado;
        await db.SaveChangesAsync();
    }
}

using TankManagement.Api.Models;
namespace TankManagement.Api.Interfaces;
public interface ISolicitudRepository
{
    Task<List<Solicitud>> ObtenerPendientesAsync();
    Task<Solicitud?> ObtenerPorIdAsync(int id);
    Task CrearAsync(Solicitud solicitud);
    Task ActualizarEstadoAsync(int id, bool estado);
}

using TankManagement.Api.Models;
namespace TankManagement.Api.Interfaces;
public interface ITanqueRepository
{
    Task<Tanque?> ObtenerPorIdAsync(int id);
    Task<List<Tanque>> ObtenerTodosAsync();
    Task<List<Tanque>> ObtenerPorUsuarioIdAsync(int usuarioId);
    Task CrearAsync(Tanque tanque);
    Task EliminarAsync(int id);
    Task ActualizarCantidadAsync(int id, decimal cantidad);
}

using TankManagement.Api.Models;
namespace TankManagement.Api.Interfaces;
public interface IRolRepository
{
    Task<Rol?> ObtenerPorIdAsync(int id);
}

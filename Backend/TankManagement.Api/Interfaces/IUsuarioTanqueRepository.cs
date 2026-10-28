using TankManagement.Api.Models;
namespace TankManagement.Api.Interfaces;
public interface IUsuarioTanqueRepository
{
    Task AsignarAsync(int usuarioId, int tanqueId);
    Task DesasignarAsync(int usuarioId, int tanqueId);
}

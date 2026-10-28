using TankManagement.Api.Models;
namespace TankManagement.Api.Interfaces;
public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorIdAsync(int id);
    Task<Usuario?> ObtenerPorUsuarioAsync(string usuario);
    Task<List<Usuario>> ObtenerTodosAsync();
    Task CrearAsync(Usuario usuario);
    Task EliminarAsync(int id);
}

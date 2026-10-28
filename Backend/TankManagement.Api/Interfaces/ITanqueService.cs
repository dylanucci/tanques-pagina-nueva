using TankManagement.Api.DTOs;
namespace TankManagement.Api.Interfaces;
public interface ITanqueService
{
    Task<TanqueResponseDto> CrearAsync(TanqueCreateDto dto);
    Task<List<TanqueResponseDto>> ObtenerTodosAsync(int? usuarioId = null);
    Task<TanqueResponseDto?> ObtenerAsync(int id);
    Task EliminarAsync(int id);
    Task AsignarAsync(int tanqueId, int usuarioId);
    Task DesasignarAsync(int tanqueId, int usuarioId);
    Task ActualizarCantidadAsync(int tanqueId, decimal cantidad);
    Task ActualizarDesdeDistanciaAsync(int tanqueId, double distanciaCm);
}

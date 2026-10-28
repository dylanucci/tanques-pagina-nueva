using TankManagement.Api.DTOs;
namespace TankManagement.Api.Interfaces;
public interface IUsuarioService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto);
    Task<UsuarioResponseDto> CrearAsync(UsuarioCreateDto dto);
    Task<List<UsuarioResponseDto>> ObtenerTodosAsync();
    Task<UsuarioResponseDto?> ObtenerAsync(int id);
    Task EliminarAsync(int id);
}

using TankManagement.Api.DTOs;
using TankManagement.Api.Interfaces;
using TankManagement.Api.Models;
namespace TankManagement.Api.Services;
public class UsuarioService(IUsuarioRepository usuarios, IRolRepository roles) : IUsuarioService
{
    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto)
    {
        var u = await usuarios.ObtenerPorUsuarioAsync(dto.Usuario);
        if (u is null || u.Contrasena != dto.Contrasena) return null;
        return new(u.Id, u.Nombre, u.UsuarioNombre, u.RolId, u.Rol.Nombre);
    }
    public async Task<UsuarioResponseDto> CrearAsync(UsuarioCreateDto dto)
    {
        if (await usuarios.ObtenerPorUsuarioAsync(dto.Usuario) is not null) throw new InvalidOperationException("El usuario ya existe.");
        if (await roles.ObtenerPorIdAsync(dto.RolId) is null) throw new InvalidOperationException("El rol no existe.");
        var u = new Usuario { Nombre = dto.Nombre.Trim(), UsuarioNombre = dto.Usuario.Trim(), Contrasena = dto.Contrasena, RolId = dto.RolId };
        await usuarios.CrearAsync(u);
        var creado = await usuarios.ObtenerPorIdAsync(u.Id) ?? throw new InvalidOperationException("No se pudo crear el usuario.");
        return Map(creado);
    }
    public async Task<List<UsuarioResponseDto>> ObtenerTodosAsync() => (await usuarios.ObtenerTodosAsync()).Select(Map).ToList();
    public async Task<UsuarioResponseDto?> ObtenerAsync(int id) { var u = await usuarios.ObtenerPorIdAsync(id); return u is null ? null : Map(u); }
    public Task EliminarAsync(int id) => usuarios.EliminarAsync(id);
    private static UsuarioResponseDto Map(Usuario u) => new(u.Id, u.Nombre, u.UsuarioNombre, u.RolId, u.Rol.Nombre);
}

using TankManagement.Api.DTOs;
using TankManagement.Api.Interfaces;
using TankManagement.Api.Models;
namespace TankManagement.Api.Services;
public class TanqueService(ITanqueRepository tanques, IUsuarioRepository usuarios, IUsuarioTanqueRepository relaciones) : ITanqueService
{
    public async Task<TanqueResponseDto> CrearAsync(TanqueCreateDto dto)
    {
        // La capacidad total no la informa el usuario: se calcula en cm³ con el método del DTO.
        var capacidadTotalCm3 = dto.CalcularCapacidadTotalCM3();
        ValidarCapacidad(dto.CapacidadActualCM3, capacidadTotalCm3);
        var t = new Tanque { TipoContenido = dto.TipoContenido.Trim(), CapacidadActualCM3 = dto.CapacidadActualCM3, CapacidadTotalCM3 = capacidadTotalCm3, Diametro = dto.Diametro, AlturaCM = dto.AlturaCM, Ubicacion = dto.Ubicacion.Trim() };
        await tanques.CrearAsync(t);
        if (dto.UsuarioId.HasValue) await AsignarAsync(t.Id, dto.UsuarioId.Value);
        return await ObtenerAsync(t.Id) ?? throw new InvalidOperationException("No se pudo crear el tanque.");
    }
    public async Task<List<TanqueResponseDto>> ObtenerTodosAsync(int? usuarioId = null) => (usuarioId.HasValue ? await tanques.ObtenerPorUsuarioIdAsync(usuarioId.Value) : await tanques.ObtenerTodosAsync()).Select(Map).ToList();
    public async Task<TanqueResponseDto?> ObtenerAsync(int id) { var t = await tanques.ObtenerPorIdAsync(id); return t is null ? null : Map(t); }
    public Task EliminarAsync(int id) => tanques.EliminarAsync(id);
    public async Task AsignarAsync(int tanqueId, int usuarioId)
    {
        if (await tanques.ObtenerPorIdAsync(tanqueId) is null) throw new KeyNotFoundException("Tanque no encontrado.");
        if (await usuarios.ObtenerPorIdAsync(usuarioId) is null) throw new KeyNotFoundException("Usuario no encontrado.");
        await relaciones.AsignarAsync(usuarioId, tanqueId);
    }
    public Task DesasignarAsync(int tanqueId, int usuarioId) => relaciones.DesasignarAsync(usuarioId, tanqueId);

    public async Task ActualizarDesdeDistanciaAsync(int tanqueId, double distanciaCm)
    {
        if (!double.IsFinite(distanciaCm) || distanciaCm < 0 || distanciaCm > 450)
            throw new ArgumentException("La distancia debe estar entre 0 y 450 cm.");

        var tanque = await tanques.ObtenerPorIdAsync(tanqueId)
            ?? throw new KeyNotFoundException("Tanque no encontrado.");

        var alturaActualCm = Math.Clamp(tanque.AlturaCM - distanciaCm, 0, tanque.AlturaCM);
        var radioCm = (double)tanque.Diametro / 2.0;
        var areaBaseCm2 = Math.PI * Math.Pow(radioCm, 2);
        var volumenCm3 = Math.Clamp(areaBaseCm2 * alturaActualCm, 0, (double)tanque.CapacidadTotalCM3);
        await ActualizarCantidadAsync(tanqueId, (decimal)Math.Round(volumenCm3, 2));
    }
    public async Task ActualizarCantidadAsync(int tanqueId, decimal capacidadActualCm3)
    {
        var t = await tanques.ObtenerPorIdAsync(tanqueId) ?? throw new KeyNotFoundException("Tanque no encontrado.");
        ValidarCapacidad(capacidadActualCm3, t.CapacidadTotalCM3);
        await tanques.ActualizarCantidadAsync(tanqueId, capacidadActualCm3);
    }
    private static void ValidarCapacidad(decimal capacidadActualCm3, decimal capacidadTotalCm3) { if (capacidadActualCm3 < 0 || capacidadTotalCm3 <= 0 || capacidadActualCm3 > capacidadTotalCm3) throw new ArgumentException("La capacidad actual debe estar entre 0 y la capacidad total del tanque (cm³)."); }
    private static TanqueResponseDto Map(Tanque t) => new(t.Id, t.TipoContenido, t.CapacidadActualCM3, t.CapacidadTotalCM3, t.Diametro, t.AlturaCM, t.Ubicacion, t.CapacidadTotalCM3 == 0 ? 0 : Math.Round(t.CapacidadActualCM3 / t.CapacidadTotalCM3 * 100, 2), t.UsuarioTanques.Select(ut => new UsuarioResponseDto(ut.Usuario.Id, ut.Usuario.Nombre, ut.Usuario.UsuarioNombre, ut.Usuario.RolId, ut.Usuario.Rol.Nombre)).ToList());
}

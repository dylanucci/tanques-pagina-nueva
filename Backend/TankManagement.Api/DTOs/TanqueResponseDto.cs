namespace TankManagement.Api.DTOs;
public record TanqueResponseDto(int Id, string TipoContenido, decimal CapacidadActualCM3, decimal CapacidadTotalCM3, decimal Diametro, double AlturaCM, string Ubicacion, decimal PorcentajeLlenado, List<UsuarioResponseDto> Usuarios);

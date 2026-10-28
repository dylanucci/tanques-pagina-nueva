namespace TankManagement.Api.DTOs;
public record SolicitudResponseDto(int Id, string Nombre, string Apellido, string Dni, string Direccion, DateTime Fecha, TimeSpan Hora, bool Estado);

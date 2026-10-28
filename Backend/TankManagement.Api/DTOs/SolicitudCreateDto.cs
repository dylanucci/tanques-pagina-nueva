using System.ComponentModel.DataAnnotations;
namespace TankManagement.Api.DTOs;
public class SolicitudCreateDto
{
    [Required, MaxLength(100)] public string Nombre { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Apellido { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string Dni { get; set; } = string.Empty;
    [Required, MaxLength(255)] public string Direccion { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public TimeSpan Hora { get; set; }
}

using System.ComponentModel.DataAnnotations;
namespace TankManagement.Api.DTOs;
public class UsuarioCreateDto
{
    [Required, MaxLength(100)] public string Nombre { get; set; } = string.Empty;
    [Required, MaxLength(50)] public string Usuario { get; set; } = string.Empty;
    [Required, MinLength(4), MaxLength(255)] public string Contrasena { get; set; } = string.Empty;
    [Range(1, int.MaxValue)] public int RolId { get; set; }
}

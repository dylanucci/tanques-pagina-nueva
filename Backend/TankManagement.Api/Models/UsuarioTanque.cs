namespace TankManagement.Api.Models;

public class UsuarioTanque
{
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public int TanqueId { get; set; }
    public Tanque Tanque { get; set; } = null!;
}

namespace TankManagement.Api.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string UsuarioNombre { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
    public int RolId { get; set; }
    public Rol Rol { get; set; } = null!;
    public ICollection<UsuarioTanque> UsuarioTanques { get; set; } = new List<UsuarioTanque>();
}

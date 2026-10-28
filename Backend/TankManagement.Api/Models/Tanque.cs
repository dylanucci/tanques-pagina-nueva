namespace TankManagement.Api.Models;

public class Tanque
{
    public int Id { get; set; }
    public string TipoContenido { get; set; } = string.Empty;
    public decimal CapacidadActualCM3 { get; set; } // Volumen de líquido actual, en cm³
    public decimal CapacidadTotalCM3 { get; set; }  // Capacidad total calculada, en cm³
    public decimal Diametro { get; set; }
    public double AlturaCM { get; set; }
    public string Ubicacion { get; set; } = string.Empty;
    public ICollection<UsuarioTanque> UsuarioTanques { get; set; } = new List<UsuarioTanque>();

    
}
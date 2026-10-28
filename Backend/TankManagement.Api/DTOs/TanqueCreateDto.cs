using System.ComponentModel.DataAnnotations;
namespace TankManagement.Api.DTOs;
public class TanqueCreateDto
{
    [Required, MaxLength(100)] public string TipoContenido { get; set; } = string.Empty;

    // El frontend envía 0 en ambas capacidades. 0 es un valor válido.
    [Range(0, double.MaxValue)] public decimal CapacidadActualCM3 { get; set; }

    // Se ignora: la capacidad total la calcula CalcularCapacidadTotalCM3 a partir del diámetro y la altura.
    [Range(0, double.MaxValue)] public decimal CapacidadTotalCM3 { get; set; }

    // Tope de 100000 cm (1 km): evita desbordes en el cálculo con valores absurdos.
    [Range(0.01, 100000)] public decimal Diametro { get; set; }
    [Range(0.01, 100000)] public double AlturaCM { get; set; }
    [Required, MaxLength(255)] public string Ubicacion { get; set; } = string.Empty;
    public int? UsuarioId { get; set; }

    /// <summary>
    /// Capacidad total del tanque en cm³ (decimal, 2 decimales) a partir de <see cref="Diametro"/> y
    /// <see cref="AlturaCM"/>. Fórmula del cilindro: V = π · r² · h.
    /// </summary>
    /// <exception cref="ArgumentException">Diámetro o altura inválidos, o tan pequeños que la capacidad es 0.</exception>
    public decimal CalcularCapacidadTotalCM3()
    {
        if (Diametro <= 0 || AlturaCM <= 0 || !double.IsFinite(AlturaCM))
            throw new ArgumentException("El diámetro y la altura del tanque deben ser mayores que 0.");

        decimal radio = Diametro / 2m;
        decimal capacidadTotalCm3 = Math.Round((decimal)Math.PI * (radio * radio) * (decimal)AlturaCM, 2);

        if (capacidadTotalCm3 <= 0)
            throw new ArgumentException("El diámetro y la altura son demasiado pequeños: la capacidad calculada es 0 cm³.");

        return capacidadTotalCm3;
    }
}

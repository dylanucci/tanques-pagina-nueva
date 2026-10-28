using Microsoft.AspNetCore.Mvc;
using TankManagement.Api.Interfaces;

namespace TankManagement.Api.Controllers;

[ApiController, Route("api/arduino")]
public class ArduinoController(ITanqueService tanques) : ControllerBase
{
    // Se conserva para integraciones HTTP antiguas; la lectura principal ahora entra directamente por COM3.
    [HttpPost("tanque")]
    public async Task<IActionResult> Actualizar([FromQuery] int tanqueId, [FromQuery] double distancia)
    {
        if (tanqueId <= 0)
            return BadRequest(new { success = false, message = "El ID del tanque es obligatorio." });
        if (!double.IsFinite(distancia) || distancia < 0 || distancia > 450)
            return BadRequest(new { success = false, message = "La distancia debe estar entre 0 y 450 cm." });

        try
        {
            await tanques.ActualizarDesdeDistanciaAsync(tanqueId, distancia);
            var tanque = await tanques.ObtenerAsync(tanqueId);
            var alturaActualCm = Math.Clamp(tanque!.AlturaCM - distancia, 0, tanque.AlturaCM);
            return Ok(new
            {
                success = true,
                tanqueId,
                distanciaCm = distancia,
                alturaActualCm = Math.Round(alturaActualCm, 2),
                capacidadActualCM3 = tanque.CapacidadActualCM3
            });
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new { success = false, message = e.Message });
        }
        catch (ArgumentException e)
        {
            return BadRequest(new { success = false, message = e.Message });
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using TankManagement.Api.DTOs;
using TankManagement.Api.Interfaces;
namespace TankManagement.Api.Controllers;
[ApiController, Route("api/tanques")]
public class TanquesController(ITanqueService tanques, IArduinoControlService arduino) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Crear(TanqueCreateDto dto)
    {
        try
        {
            var t = await tanques.CrearAsync(dto); // el servicio calcula la capacidad
            return CreatedAtAction(nameof(Obtener), new { id = t.Id }, t);
        }
        catch (Exception e) when (e is ArgumentException or InvalidOperationException)
        {
            return BadRequest(new { success = false, message = e.Message });
        }
    }

    [HttpPost("{id:int}/auto-llenado")]
    public async Task<IActionResult> AutoLlenado(int id, CancellationToken cancellationToken)
    {
        if (id <= 0) return BadRequest(new { success = false, message = "El ID del tanque no es válido." });
        if (await tanques.ObtenerAsync(id) is null)
            return NotFound(new { success = false, message = "Tanque no encontrado." });

        var resultado = await arduino.SolicitarLlenadoHasta80Async(id, cancellationToken);
        if (!resultado.Success)
        {
            var status = arduino.IsConnected ? StatusCodes.Status502BadGateway : StatusCodes.Status503ServiceUnavailable;
            return StatusCode(status, new { success = false, message = resultado.Message });
        }

        return Accepted(new
        {
            success = true,
            tanqueId = id,
            objetivoPorcentaje = 80,
            message = "Arduino confirmó el comando de llenado hasta el 80 %.",
            estado = resultado.Message
        });
    }

    [HttpGet] public async Task<IActionResult> ObtenerTodos([FromQuery] int? usuarioId) => Ok(await tanques.ObtenerTodosAsync(usuarioId));
    [HttpGet("{id:int}")] public async Task<IActionResult> Obtener(int id) { var t = await tanques.ObtenerAsync(id); return t is null ? NotFound() : Ok(t); }
    [HttpDelete("{id:int}")] public async Task<IActionResult> Eliminar(int id) { await tanques.EliminarAsync(id); return NoContent(); }
    [HttpPost("{id:int}/usuarios/{usuarioId:int}")] public async Task<IActionResult> Asignar(int id, int usuarioId) { try { await tanques.AsignarAsync(id, usuarioId); return Ok(new { success=true }); } catch (KeyNotFoundException e) { return NotFound(new { success=false, message=e.Message }); } }
    [HttpDelete("{id:int}/usuarios/{usuarioId:int}")] public async Task<IActionResult> Desasignar(int id, int usuarioId) { await tanques.DesasignarAsync(id, usuarioId); return NoContent(); }
}

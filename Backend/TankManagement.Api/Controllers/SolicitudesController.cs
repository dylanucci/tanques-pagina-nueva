using Microsoft.AspNetCore.Mvc;
using TankManagement.Api.DTOs;
using TankManagement.Api.Interfaces;
using TankManagement.Api.Models;
namespace TankManagement.Api.Controllers;
[ApiController, Route("api/solicitudes")]
public class SolicitudesController(ISolicitudRepository solicitudes) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Crear(SolicitudCreateDto dto)
    {
        var solicitud = new Solicitud
        {
            Nombre = dto.Nombre.Trim(), Apellido = dto.Apellido.Trim(), Dni = dto.Dni.Trim(), Direccion = dto.Direccion.Trim(),
            Fecha = dto.Fecha.Date, Hora = dto.Hora, Estado = false
        };
        await solicitudes.CrearAsync(solicitud);
        return CreatedAtAction(nameof(Obtener), new { id = solicitud.Id }, Map(solicitud));
    }

    [HttpGet("pendientes")]
    public async Task<IActionResult> ObtenerPendientes() => Ok((await solicitudes.ObtenerPendientesAsync()).Select(Map));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obtener(int id)
    {
        var solicitud = await solicitudes.ObtenerPorIdAsync(id);
        return solicitud is null ? NotFound(new { success = false, message = "Solicitud no encontrada." }) : Ok(Map(solicitud));
    }

    [HttpPut("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, SolicitudEstadoDto dto)
    {
        try { await solicitudes.ActualizarEstadoAsync(id, dto.Estado); return Ok(new { success = true }); }
        catch (KeyNotFoundException e) { return NotFound(new { success = false, message = e.Message }); }
    }

    private static SolicitudResponseDto Map(Solicitud x) => new(x.Id, x.Nombre, x.Apellido, x.Dni, x.Direccion, x.Fecha, x.Hora, x.Estado);
}

using Microsoft.AspNetCore.Mvc;
using TankManagement.Api.DTOs;
using TankManagement.Api.Interfaces;
namespace TankManagement.Api.Controllers;
[ApiController, Route("api/usuarios")]
public class UsuariosController(IUsuarioService usuarios) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Crear(UsuarioCreateDto dto)
    {
        try { var creado = await usuarios.CrearAsync(dto); return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado); }
        catch (InvalidOperationException e) { return Conflict(new { success=false, message=e.Message }); }
    }
    [HttpGet] public async Task<IActionResult> ObtenerTodos() => Ok(await usuarios.ObtenerTodosAsync());
    [HttpGet("{id:int}")] public async Task<IActionResult> Obtener(int id) { var u = await usuarios.ObtenerAsync(id); return u is null ? NotFound() : Ok(u); }
    [HttpDelete("{id:int}")] public async Task<IActionResult> Eliminar(int id) { await usuarios.EliminarAsync(id); return NoContent(); }
}

using Microsoft.AspNetCore.Mvc;
using TankManagement.Api.DTOs;
using TankManagement.Api.Interfaces;
namespace TankManagement.Api.Controllers;
[ApiController, Route("api/auth")]
public class AuthController(IUsuarioService usuarios) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto dto)
    {
        var result = await usuarios.LoginAsync(dto);
        return result is null ? Unauthorized(new { success = false, message = "Usuario o contraseña incorrectos." }) : Ok(new { success = true, data = result });
    }
}

namespace TankManagement.Api.Interfaces;

public sealed record ArduinoCommandResult(bool Success, string Message);

public interface IArduinoControlService
{
    bool IsConnected { get; }
    Task<ArduinoCommandResult> SolicitarLlenadoHasta80Async(int tanqueId, CancellationToken cancellationToken = default);
}

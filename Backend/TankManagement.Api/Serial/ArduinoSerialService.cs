using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO.Ports;
using System.Text;
using Microsoft.Extensions.Options;
using TankManagement.Api.Configuration;
using TankManagement.Api.Interfaces;

namespace TankManagement.Api.Serial;

/// <summary>
/// Mantiene COM3 abierto dentro de la propia API. Las líneas de telemetría se actualizan
/// directamente en la base de datos; los comandos HTTP se escriben al mismo puerto serie.
/// </summary>
public sealed class ArduinoSerialService : BackgroundService, IArduinoControlService
{
    private readonly ArduinoSerialOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ArduinoSerialService> _logger;
    private readonly object _portLock = new();
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _pendingCommands = new();
    private SerialPort? _port;
    private volatile bool _isConnected;

    public ArduinoSerialService(
        IOptions<ArduinoSerialOptions> options,
        IServiceScopeFactory scopeFactory,
        ILogger<ArduinoSerialService> logger)
    {
        _options = options.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public bool IsConnected => _isConnected;

    public async Task<ArduinoCommandResult> SolicitarLlenadoHasta80Async(int tanqueId, CancellationToken cancellationToken = default)
    {
        if (tanqueId <= 0)
            return new(false, "El ID del tanque debe ser mayor que cero.");

        var key = $"{tanqueId}:FILL80";
        var completion = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (!_pendingCommands.TryAdd(key, completion))
            return new(false, "Ya hay una solicitud de llenado pendiente para este tanque.");

        try
        {
            lock (_portLock)
            {
                if (_port is null || !_port.IsOpen || !_isConnected)
                    return new(false, $"El Arduino no está conectado al puerto {_options.PortName}.");

                _port.WriteLine($"CMD;{tanqueId.ToString(CultureInfo.InvariantCulture)};FILL80");
                _logger.LogInformation("Comando de auto llenado enviado al tanque {TankId} por {Port}.", tanqueId, _options.PortName);
            }

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(_options.CommandTimeoutSeconds));
            string acknowledgement;
            try
            {
                acknowledgement = await completion.Task.WaitAsync(timeout.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return new(false, "El Arduino no confirmó el comando a tiempo. Comprueba el ID del tanque y el sketch cargado.");
            }

            var parts = acknowledgement.Split(';');
            if (parts.Length >= 4 && parts[3].Equals("ERROR", StringComparison.OrdinalIgnoreCase))
                return new(false, parts.Length >= 5 ? parts[4] : "El Arduino rechazó el comando.");

            return new(true, parts.Length >= 4 ? parts[3] : "Comando confirmado por el Arduino.");
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or UnauthorizedAccessException or TimeoutException)
        {
            _logger.LogWarning(ex, "No se pudo enviar el comando de llenado al tanque {TankId}.", tanqueId);
            return new(false, $"No se pudo escribir en {_options.PortName}: {ex.Message}");
        }
        finally
        {
            _pendingCommands.TryRemove(key, out _);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Servicio serial de Arduino iniciado. Puerto {Port}, {Baud} baudios.", _options.PortName, _options.BaudRate);
        var consecutiveFailures = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            SerialPort? port = null;
            try
            {
                port = new SerialPort(_options.PortName, _options.BaudRate, _options.Parity, _options.DataBits, _options.StopBits)
                {
                    DtrEnable = _options.DtrEnable,
                    Encoding = Encoding.ASCII,
                    NewLine = "\n",
                    ReadTimeout = 1000,
                    WriteTimeout = 1500
                };

                port.Open();
                consecutiveFailures = 0;
                port.DiscardInBuffer();
                lock (_portLock) _port = port;
                _isConnected = true;
                var lastDataTimestamp = Stopwatch.GetTimestamp();
                _logger.LogInformation("Arduino conectado en {Port}; telemetría y comandos gestionados por la API.", _options.PortName);

                while (!stoppingToken.IsCancellationRequested && port.IsOpen)
                {
                    try
                    {
                        var line = port.ReadLine().Trim();
                        if (line.Length == 0) continue;
                        Interlocked.Exchange(ref lastDataTimestamp, Stopwatch.GetTimestamp());
                        if (line.Length > _options.MaxLineLength)
                        {
                            _logger.LogWarning("Se ignoró una línea serial demasiado larga ({Length} caracteres).", line.Length);
                            continue;
                        }

                        if (line.StartsWith("ACK;", StringComparison.OrdinalIgnoreCase))
                        {
                            HandleAcknowledgement(line);
                            continue;
                        }

                        await HandleTelemetryAsync(line, stoppingToken);
                    }
                    catch (TimeoutException)
                    {
                        // ReadLine usa un timeout corto para poder comprobar la cancelación regularmente.
                        if (_options.NoDataTimeoutSeconds > 0
                            && Stopwatch.GetElapsedTime(Interlocked.Read(ref lastDataTimestamp)) > TimeSpan.FromSeconds(_options.NoDataTimeoutSeconds))
                            throw new TimeoutException($"No se reciben líneas del Arduino desde hace {_options.NoDataTimeoutSeconds} segundos.");
                    }
                }

                if (!stoppingToken.IsCancellationRequested)
                    throw new IOException("El puerto serie se cerró inesperadamente.");
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                consecutiveFailures++;
                var warnEvery = Math.Max(1, 60 / _options.ReconnectDelaySeconds);
                if (consecutiveFailures == 1 || consecutiveFailures % warnEvery == 0)
                    _logger.LogWarning("Puerto {Port} no disponible o desconectado ({Reason}). Se reintentará en {Delay} s (fallo #{Count}).", _options.PortName, ex.Message, _options.ReconnectDelaySeconds, consecutiveFailures);
                else
                    _logger.LogDebug("Reintento de puerto {Port}: {Reason}", _options.PortName, ex.Message);
                _logger.LogDebug(ex, "Detalle del error serial.");
                FailPendingCommands("El puerto serie se desconectó antes de confirmar el comando.");
            }
            finally
            {
                _isConnected = false;
                lock (_portLock)
                {
                    if (ReferenceEquals(_port, port)) _port = null;
                }
                try { if (port?.IsOpen == true) port.Close(); } catch { /* puerto ya desconectado */ }
                port?.Dispose();
            }

            try { await Task.Delay(TimeSpan.FromSeconds(_options.ReconnectDelaySeconds), stoppingToken); }
            catch (OperationCanceledException) { break; }
        }

        _logger.LogInformation("Servicio serial de Arduino detenido.");
    }

    private void HandleAcknowledgement(string line)
    {
        var parts = line.Split(';');
        if (parts.Length < 4 || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var tankId))
        {
            _logger.LogWarning("Confirmación serial inválida: {Line}", line);
            return;
        }

        var command = parts[2].Trim();
        var key = $"{tankId}:{command}";
        if (_pendingCommands.TryGetValue(key, out var completion))
            completion.TrySetResult(line);
        else
            _logger.LogDebug("Confirmación serial sin solicitud pendiente: {Line}", line);
    }

    private async Task HandleTelemetryAsync(string line, CancellationToken cancellationToken)
    {
        var parts = line.Split(';');
        int tankId;
        string distanceText;
        if (parts.Length == 2)
        {
            if (!int.TryParse(parts[0].Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out tankId) || tankId <= 0)
            {
                _logger.LogWarning("Línea serial ignorada: {Line}", line);
                return;
            }
            distanceText = parts[1].Trim();
        }
        else if (parts.Length == 1)
        {
            // Se conserva la compatibilidad con sketches que emiten únicamente la distancia.
            tankId = _options.DefaultTankId;
            distanceText = parts[0].Trim();
        }
        else
        {
            _logger.LogWarning("Línea serial ignorada: {Line}", line);
            return;
        }

        distanceText = distanceText.Replace(',', '.');
        if (!double.TryParse(distanceText, NumberStyles.Float, CultureInfo.InvariantCulture, out var distance)
            || !double.IsFinite(distance) || distance < 0 || distance > _options.MaxDistanceCm)
        {
            _logger.LogWarning("Distancia serial inválida para tanque {TankId}: {Value}", tankId, distanceText);
            return;
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var tankService = scope.ServiceProvider.GetRequiredService<ITanqueService>();
            await tankService.ActualizarDesdeDistanciaAsync(tankId, distance);
            _logger.LogDebug("Telemetría actualizada: tanque {TankId}, distancia {Distance:F2} cm.", tankId, distance);
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("Se recibió telemetría de un tanque inexistente (ID {TankId}).", tankId);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "La telemetría del tanque {TankId} no pudo actualizarse.", tankId);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error guardando la telemetría del tanque {TankId}.", tankId);
        }
    }

    private void FailPendingCommands(string message)
    {
        foreach (var pending in _pendingCommands.Values)
            pending.TrySetException(new IOException(message));
    }
}

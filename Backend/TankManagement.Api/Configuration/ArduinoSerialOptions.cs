using System.ComponentModel.DataAnnotations;
using System.IO.Ports;

namespace TankManagement.Api.Configuration;

/// <summary>Configuración del Arduino conectado directamente a la API.</summary>
public sealed class ArduinoSerialOptions
{
    public const string SectionName = "ArduinoSerial";

    [Required]
    public string PortName { get; set; } = "COM3";

    [Range(300, 4_000_000)]
    public int BaudRate { get; set; } = 115200;

    [Range(5, 8)]
    public int DataBits { get; set; } = 8;

    public Parity Parity { get; set; } = Parity.None;
    public StopBits StopBits { get; set; } = StopBits.One;
    public bool DtrEnable { get; set; } = true;

    [Range(1, 3600)]
    public int ReconnectDelaySeconds { get; set; } = 5;

    [Range(1, 30)]
    public int CommandTimeoutSeconds { get; set; } = 4;

    [Range(1, int.MaxValue)]
    public int DefaultTankId { get; set; } = 1;

    [Range(1, 100000)]
    public double MaxDistanceCm { get; set; } = 450;

    [Range(0, 86400)]
    public int NoDataTimeoutSeconds { get; set; } = 60;

    [Range(16, 65536)]
    public int MaxLineLength { get; set; } = 256;
}

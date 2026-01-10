namespace FutabaTelemetryTrace.Models;

/// <summary>
/// Represents a telemetry channel with metadata
/// </summary>
public class TelemetryChannel
{
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public double MinValue { get; set; }
    public double MaxValue { get; set; }
    public System.Windows.Media.Color DisplayColor { get; set; }
}

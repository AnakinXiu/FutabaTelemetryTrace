namespace FutabaTelemetryTrace.Models;

/// <summary>
/// Represents a single telemetry data point with timestamp and channel values
/// </summary>
public class TelemetryDataPoint
{
    public double Timestamp { get; set; }
    public Dictionary<string, double> ChannelValues { get; set; } = new();
}

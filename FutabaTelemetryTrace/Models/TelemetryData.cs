namespace FutabaTelemetryTrace.Models;

/// <summary>
/// Container for all telemetry data including channels and data points
/// </summary>
public class TelemetryData
{
    public List<TelemetryChannel> Channels { get; set; } = new();
    public List<TelemetryDataPoint> DataPoints { get; set; } = new();
    public double Duration => DataPoints.Count > 0 ? DataPoints[^1].Timestamp : 0;
}

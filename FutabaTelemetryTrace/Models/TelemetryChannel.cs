using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace FutabaTelemetryTrace.Models;

/// <summary>
/// Represents a telemetry channel with metadata
/// </summary>
public class TelemetryChannel : INotifyPropertyChanged
{
    private bool _isVisible = true;

    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public double MinValue { get; set; }
    public double MaxValue { get; set; }
    public Color DisplayColor { get; set; }

    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (_isVisible == value)
                return;
            _isVisible = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

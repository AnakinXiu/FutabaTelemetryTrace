using FutabaTelemetryTrace.Helpers;
using FutabaTelemetryTrace.Models;
using FutabaTelemetryTrace.Services;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.Win32;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace FutabaTelemetryTrace.ViewModels;

/// <summary>
/// Main ViewModel for the application
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly ExcelReaderService _excelReader;
    private readonly VideoExportService _videoExporter;
    private readonly DispatcherTimer _playbackTimer;
    private TelemetryData? _telemetryData;
    private double _currentTime;
    private bool _isPlaying;
    private bool _isExporting;
    private int _exportProgress;
    private string _statusMessage = "Ready";

    public MainViewModel()
    {
        _excelReader = new ExcelReaderService();
        _videoExporter = new VideoExportService();
        _playbackTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
        };
        _playbackTimer.Tick += PlaybackTimer_Tick;

        LoadFileCommand = new RelayCommand(LoadFile);
        PlayCommand = new RelayCommand(Play, () => _telemetryData != null && !_isPlaying);
        PauseCommand = new RelayCommand(Pause, () => _isPlaying);
        ResetCommand = new RelayCommand(Reset, () => _telemetryData != null);
        ExportVideoCommand = new RelayCommand(async () => await ExportVideoAsync(), () => _telemetryData != null && !_isExporting);

        Series = new ObservableCollection<ISeries>();
    }

    public ICommand LoadFileCommand { get; }
    public ICommand PlayCommand { get; }
    public ICommand PauseCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand ExportVideoCommand { get; }

    public ObservableCollection<ISeries> Series { get; }

    public double CurrentTime
    {
        get => _currentTime;
        set
        {
            if (SetProperty(ref _currentTime, value))
            {
                UpdateChart();
            }
        }
    }

    public double MaxTime => _telemetryData?.Duration ?? 0;

    public bool IsPlaying
    {
        get => _isPlaying;
        set => SetProperty(ref _isPlaying, value);
    }

    public bool IsExporting
    {
        get => _isExporting;
        set => SetProperty(ref _isExporting, value);
    }

    public int ExportProgress
    {
        get => _exportProgress;
        set => SetProperty(ref _exportProgress, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    private void LoadFile()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
            Title = "Select Telemetry Data File"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                StatusMessage = "Loading file...";
                _telemetryData = _excelReader.ReadFromExcel(dialog.FileName);
                
                Series.Clear();
                foreach (var channel in _telemetryData.Channels)
                {
                    var series = new LineSeries<ObservablePoint>
                    {
                        Name = channel.Name,
                        Values = new ObservableCollection<ObservablePoint>(),
                        Stroke = new SolidColorPaint(new SKColor(
                            channel.DisplayColor.R,
                            channel.DisplayColor.G,
                            channel.DisplayColor.B
                        )) { StrokeThickness = 2 },
                        GeometrySize = 0,
                        LineSmoothness = 0
                    };
                    Series.Add(series);
                }

                CurrentTime = 0;
                OnPropertyChanged(nameof(MaxTime));
                StatusMessage = $"Loaded {_telemetryData.DataPoints.Count} data points from {_telemetryData.Channels.Count} channels";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = "Error loading file";
            }
        }
    }

    private void Play()
    {
        IsPlaying = true;
        _playbackTimer.Start();
        StatusMessage = "Playing...";
    }

    private void Pause()
    {
        IsPlaying = false;
        _playbackTimer.Stop();
        StatusMessage = "Paused";
    }

    private void Reset()
    {
        Pause();
        CurrentTime = 0;
        StatusMessage = "Ready";
    }

    private void PlaybackTimer_Tick(object? sender, EventArgs e)
    {
        if (_telemetryData == null) return;

        CurrentTime += 0.016; // Advance by ~16ms

        if (CurrentTime >= _telemetryData.Duration)
        {
            CurrentTime = _telemetryData.Duration;
            Pause();
        }
    }

    private void UpdateChart()
    {
        if (_telemetryData == null) return;

        var dataPointsToShow = _telemetryData.DataPoints
            .Where(dp => dp.Timestamp <= CurrentTime)
            .ToList();

        for (int i = 0; i < _telemetryData.Channels.Count; i++)
        {
            var channel = _telemetryData.Channels[i];
            var series = Series[i] as LineSeries<ObservablePoint>;
            if (series?.Values is ObservableCollection<ObservablePoint> values)
            {
                values.Clear();
                foreach (var point in dataPointsToShow)
                {
                    if (point.ChannelValues.TryGetValue(channel.Name, out double value))
                    {
                        values.Add(new ObservablePoint(point.Timestamp, value));
                    }
                }
            }
        }
    }

    private async Task ExportVideoAsync()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "MP4 Video (*.mp4)|*.mp4",
            Title = "Export Video",
            FileName = "telemetry_export.mp4"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                IsExporting = true;
                ExportProgress = 0;
                StatusMessage = "Exporting video...";

                // This is a placeholder - actual implementation would require
                // rendering the chart to bitmaps and passing to video encoder
                await Task.Delay(1000); // Simulate export

                MessageBox.Show("Video export is not yet fully implemented in this version.\n\nThis feature requires additional rendering infrastructure to capture chart frames.", 
                    "Not Implemented", MessageBoxButton.OK, MessageBoxImage.Information);

                StatusMessage = "Export cancelled";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting video: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = "Export failed";
            }
            finally
            {
                IsExporting = false;
                ExportProgress = 0;
            }
        }
    }
}

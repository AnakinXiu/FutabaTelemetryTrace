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
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
    private readonly object _chartUpdateLock = new();
    private TelemetryData? _telemetryData;
    private CancellationTokenSource? _chartUpdateCts;
    private FrameworkElement? _chartElement;
    private int _currentExportFps = 30;
    private double _currentTime;
    private double _windowDuration;
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
            Interval = TimeSpan.FromMilliseconds(8) // ~120 FPS
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
    public ObservableCollection<TelemetryChannel> Channels { get; } = new();
    public FrameworkElement? ChartElement
    {
        get => _chartElement;
        set => SetProperty(ref _chartElement, value);
    }

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

    public double WindowDuration
    {
        get => _windowDuration;
        set
        {
            var maxDuration = MaxTime;
            var clamped = maxDuration > 0
                ? Math.Max(0, Math.Min(value, maxDuration))
                : Math.Max(0, value);
            if (SetProperty(ref _windowDuration, clamped))
            {
                UpdateChart();
            }
        }
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
                ClearChannelSubscriptions();
                Series.Clear();
                CancelPendingChartUpdate();

                _telemetryData = _excelReader.ReadFromExcel(dialog.FileName);

                var index = 0;
                foreach (var channel in _telemetryData.Channels)
                {
                    channel.IsVisible = index < 2;
                    channel.PropertyChanged += Channel_PropertyChanged;
                    Channels.Add(channel);

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

                    index++;
                }

                CurrentTime = 0;
                OnPropertyChanged(nameof(MaxTime));
                WindowDuration = 5; //_telemetryData.Duration;
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

        CurrentTime += 0.008; // Advance by ~8ms

        if (CurrentTime >= _telemetryData.Duration)
        {
            CurrentTime = _telemetryData.Duration;
            Pause();
        }
    }

    private void UpdateChart()
    {
        var telemetryData = _telemetryData;
        if (telemetryData == null)
            return;

        CancellationTokenSource cts;
        lock (_chartUpdateLock)
        {
            _chartUpdateCts?.Cancel();
            _chartUpdateCts?.Dispose();
            _chartUpdateCts = new CancellationTokenSource();
            cts = _chartUpdateCts;
        }

        var windowSize = WindowDuration;
        if (windowSize <= 0)
        {
            windowSize = telemetryData.Duration;
        }

        var windowStart = CurrentTime;
        var windowEnd = Math.Min(windowStart + windowSize, telemetryData.Duration);

        var token = cts.Token;
        Task.Run(() =>
        {
            try
            {
                var dataPointsToShow = telemetryData.DataPoints
                    .Where(dp => dp.Timestamp >= windowStart && dp.Timestamp <= windowEnd)
                    .ToList();

                token.ThrowIfCancellationRequested();

                var channelData = new List<IReadOnlyList<ObservablePoint>>(telemetryData.Channels.Count);
                foreach (var channel in telemetryData.Channels)
                {
                    if (!channel.IsVisible)
                    {
                        channelData.Add(Array.Empty<ObservablePoint>());
                        continue;
                    }

                    var points = new List<ObservablePoint>(dataPointsToShow.Count);
                    foreach (var point in dataPointsToShow)
                    {
                        if (point.ChannelValues.TryGetValue(channel.Name, out var value))
                        {
                            points.Add(new ObservablePoint(point.Timestamp, value));
                        }
                    }
                    channelData.Add(points);
                }

                token.ThrowIfCancellationRequested();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (token.IsCancellationRequested)
                        return;

                    for (var i = 0; i < channelData.Count && i < Series.Count; i++)
                    {
                        if (Series[i] is LineSeries<ObservablePoint> lineSeries)
                        {
                            lineSeries.Values = channelData[i];
                        }
                    }
                });
            }
            catch (OperationCanceledException)
            {
                // Ignore cancelled updates
            }
        }, token);
    }

    private void ClearChannelSubscriptions()
    {
        CancelPendingChartUpdate();
        foreach (var channel in Channels)
        {
            channel.PropertyChanged -= Channel_PropertyChanged;
        }
        Channels.Clear();
    }

    private void CancelPendingChartUpdate()
    {
        lock (_chartUpdateLock)
        {
            _chartUpdateCts?.Cancel();
            _chartUpdateCts?.Dispose();
            _chartUpdateCts = null;
        }
    }

    private void Channel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TelemetryChannel.IsVisible))
        {
            UpdateChart();
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
            if (_telemetryData == null)
            {
                MessageBox.Show("Load telemetry data before exporting.", "Export", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ChartElement == null)
            {
                MessageBox.Show("Chart is not ready for capture.", "Export", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsExporting = true;
                ExportProgress = 0;
                StatusMessage = "Exporting video...";

                var chart = ChartElement;
                const int fps = 30;
                _currentExportFps = fps;
                var totalFrames = Math.Max(1, (int)Math.Ceiling(_telemetryData.Duration * fps));

                if (chart.ActualWidth <= 0 || chart.ActualHeight <= 0)
                {
                    chart.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    chart.Arrange(new Rect(chart.DesiredSize));
                }

                var width = Math.Max(1, (int)Math.Ceiling(chart.ActualWidth));
                var height = Math.Max(1, (int)Math.Ceiling(chart.ActualHeight));
                var wasPlaying = IsPlaying;
                Pause();

                var progress = new Progress<int>(frame =>
                {
                    if (totalFrames == 0)
                        return;
                    ExportProgress = (int)Math.Clamp(Math.Round(frame * 100d / totalFrames), 0, 100);
                });

                await _videoExporter.ExportToVideoAsync(CaptureChartFrameAsync, totalFrames, fps, dialog.FileName, width, height, progress);

                StatusMessage = "Export completed";
                if (wasPlaying)
                {
                    Play();
                }
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

    private Task<BitmapSource> CaptureChartFrameAsync(int frameIndex)
    {
        var chart = ChartElement ?? throw new InvalidOperationException("Chart element is not available for capture.");
        if (_telemetryData == null)
        {
            throw new InvalidOperationException("Telemetry data is required before exporting.");
        }

        if (!Application.Current.Dispatcher.CheckAccess())
        {
            return Application.Current.Dispatcher.InvokeAsync(() => CaptureChartFrameInternal(chart, frameIndex)).Task;
        }

        return Task.FromResult(CaptureChartFrameInternal(chart, frameIndex));
    }

    private BitmapSource CaptureChartFrameInternal(FrameworkElement chart, int frameIndex)
    {
        var fps = Math.Max(1, _currentExportFps);
        var targetTime = Math.Min(frameIndex / (double)fps, MaxTime);
        CurrentTime = targetTime;

        if (chart.ActualWidth <= 0 || chart.ActualHeight <= 0)
        {
            chart.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            chart.Arrange(new Rect(chart.DesiredSize));
        }

        var width = Math.Max(1, (int)Math.Ceiling(chart.ActualWidth));
        var height = Math.Max(1, (int)Math.Ceiling(chart.ActualHeight));
        var dpi = VisualTreeHelper.GetDpi(chart);
        var rtb = new RenderTargetBitmap(width, height, dpi.PixelsPerInchX, dpi.PixelsPerInchY, PixelFormats.Pbgra32);
        rtb.Render(chart);
        return rtb;
    }

}

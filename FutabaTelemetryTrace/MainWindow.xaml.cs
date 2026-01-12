using System.Windows;
using FutabaTelemetryTrace.ViewModels;

namespace FutabaTelemetryTrace;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainViewModel();
        DataContext = _viewModel;

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel.ChartElement = TelemetryChart;
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "Futaba Telemetry Trace\n\n" +
            "A tool to convert exported Futaba 10PX transmitter telemetry logs into timeline trace charts.\n\n" +
            "The result can be exported to a real-time video and combined with racing record videos for analysis.\n\n" +
            "Version 1.0",
            "About",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
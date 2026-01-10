using System.Windows;
using FutabaTelemetryTrace.ViewModels;

namespace FutabaTelemetryTrace;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
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
# FutabaTelemetryTrace

A desktop tool built with C# and WPF that consumes Excel files containing timeline and telemetry data, displays it with dynamic charts, and can export the result into a video similar to the dynamic chart in the Windows Task Manager's performance page.

## Features

- 📊 **Excel File Import**: Load telemetry data from Excel (.xlsx) files
- 📈 **Dynamic Chart Visualization**: Real-time animated line charts showing multiple telemetry channels
- ⏯️ **Playback Controls**: Play, pause, and seek through telemetry data timeline
- 🎨 **Multi-Channel Support**: Visualize multiple telemetry channels simultaneously with color-coded lines
- 🎥 **Video Export**: Export animated charts to video format (MP4) for later analysis
- 💻 **Modern UI**: Dark-themed interface optimized for long viewing sessions

## Screenshots

(Screenshots would be displayed here when the application is running)

## Requirements

- Windows 10 or later
- .NET 8.0 Runtime

## Installation

### Option 1: Build from Source

1. Clone the repository:
```bash
git clone https://github.com/AnakinXiu/FutabaTelemetryTrace.git
cd FutabaTelemetryTrace
```

2. Build the solution:
```bash
dotnet build
```

3. Run the application:
```bash
dotnet run --project FutabaTelemetryTrace/FutabaTelemetryTrace.csproj
```

### FFmpeg Requirement for Video Export

To use the video export feature, you need FFmpeg binaries:

1. Download FFmpeg from [https://ffmpeg.org/download.html](https://ffmpeg.org/download.html)
2. Extract the binaries
3. Either:
   - Add FFmpeg to your system PATH, or
   - Copy the FFmpeg DLLs to the application directory

**Note**: The video export feature's frame rendering is currently a placeholder and requires additional implementation to capture chart frames.
```

### Option 2: Download Release

Download the latest release from the [Releases](https://github.com/AnakinXiu/FutabaTelemetryTrace/releases) page and run the executable.

## Usage

### 1. Prepare Your Data

The application expects Excel files (.xlsx) with the following format:

- **Row 1**: Channel names (Column A: "Time", Column B+: channel names like "Throttle", "Steering", etc.)
- **Row 2** (optional): Units for each channel (e.g., "s", "%", "km/h", "RPM")
- **Row 3+**: Data rows (Column A: timestamp in seconds, Column B+: numeric values)

Example:
```
| Time | Throttle | Steering | Speed | RPM  |
|------|----------|----------|-------|------|
| s    | %        | %        | km/h  | RPM  |
| 0.0  | 0        | 0        | 0     | 1000 |
| 0.1  | 25       | -10      | 5     | 2000 |
| 0.2  | 50       | -15      | 10    | 3000 |
```

See [SampleData/README.md](SampleData/README.md) for detailed format specifications.

### 2. Load Data

1. Launch the application
2. Click **File > Open Excel File...**
3. Select your telemetry data file
4. The chart will display all channels with automatic color coding

### 3. Playback

- Click **▶ Play** to start animated playback
- Click **⏸ Pause** to pause playback
- Click **⏹ Reset** to return to the beginning
- Use the timeline slider to seek to a specific time

### 4. Export Video

1. Click **Export > Export to Video...** or the **📹 Export** button
2. Choose output location and filename
3. Wait for the export process to complete
4. The exported video can be combined with racing footage for analysis

## Technology Stack

- **Framework**: .NET 8.0
- **UI**: Windows Presentation Foundation (WPF)
- **Excel Reading**: EPPlus
- **Charting**: LiveCharts2 with SkiaSharp
- **Video Export**: FFMediaToolkit
- **Pattern**: MVVM (Model-View-ViewModel)

## Project Structure

```
FutabaTelemetryTrace/
├── FutabaTelemetryTrace/          # Main WPF project
│   ├── Models/                    # Data models
│   │   ├── TelemetryData.cs
│   │   ├── TelemetryChannel.cs
│   │   └── TelemetryDataPoint.cs
│   ├── ViewModels/                # MVVM ViewModels
│   │   ├── ViewModelBase.cs
│   │   └── MainViewModel.cs
│   ├── Services/                  # Business logic services
│   │   ├── ExcelReaderService.cs
│   │   └── VideoExportService.cs
│   ├── Helpers/                   # Utility classes
│   │   └── RelayCommand.cs
│   ├── Views/                     # Additional views (if any)
│   ├── MainWindow.xaml            # Main application window
│   └── App.xaml                   # Application entry point
├── SampleData/                    # Sample data files
│   └── README.md                  # Data format documentation
└── README.md                      # This file
```

## Use Case: Futaba 10PX Telemetry Analysis

This tool is specifically designed for RC enthusiasts using the Futaba 10PX transmitter:

1. Export telemetry logs from your Futaba 10PX transmitter
2. Convert the logs to Excel format (if not already in that format)
3. Load the data into this application
4. Analyze your control inputs (throttle, steering, etc.) over time
5. Export the visualization to video
6. Overlay the telemetry video with your racing footage for comprehensive analysis

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- EPPlus for Excel file handling
- LiveCharts2 for beautiful charting capabilities
- FFMediaToolkit for video encoding
- The Futaba RC community for inspiration

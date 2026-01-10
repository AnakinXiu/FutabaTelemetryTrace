# Project Architecture

## Overview

FutabaTelemetryTrace is a WPF desktop application built using the MVVM (Model-View-ViewModel) pattern. It provides visualization and analysis tools for telemetry data exported from the Futaba 10PX transmitter.

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                         MainWindow (View)                    │
│  ┌────────────────────────────────────────────────────────┐ │
│  │                    MainViewModel                        │ │
│  │  - Series: ObservableCollection<ISeries>               │ │
│  │  - CurrentTime, MaxTime, IsPlaying, StatusMessage      │ │
│  │  - LoadFileCommand, PlayCommand, ExportVideoCommand    │ │
│  └────────────────────────────────────────────────────────┘ │
│                             │                                │
│                             ▼                                │
│  ┌──────────────────┐  ┌──────────────────┐                │
│  │ ExcelReaderService│  │VideoExportService│                │
│  │ - ReadFromExcel() │  │ - ExportToVideo()│                │
│  └──────────────────┘  └──────────────────┘                │
│                             │                                │
│                             ▼                                │
│  ┌─────────────────────────────────────────────────────┐   │
│  │                   Models                             │   │
│  │  - TelemetryData                                     │   │
│  │  - TelemetryChannel                                  │   │
│  │  - TelemetryDataPoint                                │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

## Project Structure

### Models
Data classes representing the domain model:

- **TelemetryData**: Container for all telemetry information
  - List of channels
  - List of data points
  - Calculated duration

- **TelemetryChannel**: Metadata about a telemetry channel
  - Name, unit, min/max values
  - Display color for visualization

- **TelemetryDataPoint**: Single time-stamped measurement
  - Timestamp in seconds
  - Dictionary of channel values

### ViewModels

- **ViewModelBase**: Base class implementing INotifyPropertyChanged
  - Property change notification infrastructure
  - SetProperty helper method

- **MainViewModel**: Main application ViewModel
  - Manages chart data (Series collection)
  - Implements playback logic (Play, Pause, Reset)
  - Handles file loading and export
  - Coordinates between UI and services

### Services

- **ExcelReaderService**: Excel file parsing
  - Uses EPPlus library
  - Detects data format (with/without unit row)
  - Calculates min/max values for each channel
  - Assigns colors to channels

- **VideoExportService**: Video export functionality
  - Uses FFMediaToolkit library
  - Converts chart frames to video
  - Supports H.264 encoding
  - (Note: Frame rendering not fully implemented)

### Helpers

- **RelayCommand**: ICommand implementation for MVVM
  - Enables binding UI actions to ViewModel methods
  - Supports CanExecute logic

### Views

- **MainWindow.xaml**: Main application window
  - Menu bar (File, Export, Help)
  - LiveCharts CartesianChart for visualization
  - Playback controls (Play, Pause, Reset, Export)
  - Timeline slider for seeking
  - Status bar with progress indicator

## Key Technologies

### UI Framework
- **WPF (Windows Presentation Foundation)**: Windows desktop UI framework
- **XAML**: Declarative UI markup language
- **MVVM Pattern**: Separation of concerns for testability

### Data Processing
- **EPPlus 7.5.2**: Excel file reading (.xlsx)
  - Handles large datasets efficiently
  - Supports modern Excel formats

### Visualization
- **LiveCharts2**: Modern charting library
  - Real-time data updates
  - Interactive charts
  - SkiaSharp rendering backend for performance

### Video Export
- **FFMediaToolkit 4.8.1**: Video encoding
  - H.264 codec support
  - Frame-by-frame video generation

## Data Flow

### Loading Data
1. User selects Excel file via OpenFileDialog
2. ExcelReaderService parses the file:
   - Reads channel names from row 1
   - Detects units from row 2 (if present)
   - Loads data points
   - Calculates statistics
3. MainViewModel creates LiveCharts Series for each channel
4. Chart is rendered with initial state

### Playback
1. User clicks Play button
2. DispatcherTimer starts (16ms interval ≈ 60 FPS)
3. On each tick:
   - CurrentTime increments
   - UpdateChart() filters data points up to CurrentTime
   - Chart automatically re-renders with new data
4. Animation continues until end or user pauses

### Export (Planned)
1. User clicks Export button
2. VideoExportService is invoked
3. For each frame:
   - Update chart to specific timestamp
   - Render chart to bitmap
   - Encode bitmap to video frame
4. Finalize and save video file

## Design Decisions

### Why MVVM?
- Separation of UI and business logic
- Testable ViewModels without UI dependencies
- Data binding reduces boilerplate code

### Why LiveCharts2?
- Native WPF support
- Good performance with real-time updates
- Modern API and active development

### Why EPPlus?
- Widely used and well-maintained
- Handles complex Excel files
- Good performance for large datasets

### Why FFMediaToolkit?
- Simple API for video encoding
- Cross-platform .NET support
- H.264 codec for broad compatibility

## Future Enhancements

### Short Term
1. Complete video export with chart rendering
2. Add zoom/pan controls
3. Implement data export (CSV, JSON)
4. Add unit tests

### Medium Term
1. Multiple chart layouts
2. Real-time data streaming
3. Data filtering and transformation
4. Plugin architecture for custom data sources

### Long Term
1. Cloud sync for telemetry data
2. Machine learning for pattern analysis
3. Multi-device support (mobile companion app)
4. Collaborative analysis features

## Performance Considerations

### Large Datasets
- LiveCharts2 efficiently handles thousands of points
- Consider data decimation for very large files (>100k points)
- Implement virtual scrolling for timeline

### Memory Usage
- Excel files are loaded entirely into memory
- Consider streaming for very large files
- Dispose of resources properly (IDisposable pattern)

### Rendering
- SkiaSharp provides hardware-accelerated rendering
- Use lower frame rate for smoother playback of complex charts
- Consider separate rendering thread for export

## Debugging Tips

### Common Issues
1. **Chart not displaying**: Check that Series collection is populated
2. **Playback not smooth**: Adjust DispatcherTimer interval
3. **Excel loading fails**: Verify file format matches expected structure
4. **Video export incomplete**: Ensure FFmpeg binaries are available

### Logging
- Add console output in development mode
- Use Debug.WriteLine for troubleshooting
- Consider adding a logging framework (Serilog, NLog)

## Testing Strategy

### Unit Tests (Recommended)
- Test ExcelReaderService with sample files
- Test data model calculations
- Test ViewModel commands and properties

### Integration Tests
- Test end-to-end file loading
- Test playback functionality
- Test export process

### Manual Testing
- Test with various Excel formats
- Test with large datasets
- Test edge cases (empty files, invalid data)

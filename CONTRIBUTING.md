# Contributing to Futaba Telemetry Trace

Thank you for your interest in contributing to Futaba Telemetry Trace!

## Getting Started

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/YOUR-USERNAME/FutabaTelemetryTrace.git
   ```
3. **Create a branch** for your changes:
   ```bash
   git checkout -b feature/your-feature-name
   ```

## Development Setup

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022, VS Code, or Rider (recommended for WPF development)
- Windows 10 or later (required for WPF)

### Building the Project

```bash
dotnet restore
dotnet build
```

### Running the Application

```bash
dotnet run --project FutabaTelemetryTrace/FutabaTelemetryTrace.csproj
```

## Code Style

- Follow standard C# coding conventions
- Use meaningful variable and method names
- Add XML documentation comments to public APIs
- Keep methods small and focused on a single responsibility

## Testing

Before submitting a pull request:

1. Build the solution without warnings
2. Test the application with sample data files
3. Verify all features work as expected:
   - Excel file loading
   - Chart visualization
   - Playback controls
   - Export functionality (when implemented)

## Submitting Changes

1. **Commit your changes** with clear commit messages:
   ```bash
   git add .
   git commit -m "Add feature: description of your changes"
   ```
2. **Push to your fork**:
   ```bash
   git push origin feature/your-feature-name
   ```
3. **Create a Pull Request** on GitHub with:
   - Clear description of changes
   - Screenshots if UI changes are involved
   - Reference to any related issues

## Areas for Contribution

Here are some areas where contributions are welcome:

### High Priority
- Complete video export functionality with actual chart rendering
- Add support for more data formats (CSV, JSON)
- Improve chart performance for large datasets
- Add unit tests

### Medium Priority
- Add zoom and pan controls to the chart
- Implement data filtering options
- Add export to image formats (PNG, SVG)
- Improve error handling and user feedback

### Nice to Have
- Dark/Light theme toggle
- Customizable chart colors
- Multiple chart layouts (line, area, bar)
- Real-time data streaming support

## Questions?

Feel free to open an issue for:
- Bug reports
- Feature requests
- Questions about the codebase
- Suggestions for improvements

## License

By contributing to this project, you agree that your contributions will be licensed under the MIT License.

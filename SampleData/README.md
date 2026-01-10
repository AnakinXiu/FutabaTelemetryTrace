# Sample Telemetry Data Format

This directory contains sample Excel files that demonstrate the expected format for telemetry data.

## Excel File Format

The application expects Excel files (.xlsx) with the following structure:

### Row 1: Channel Names (Required)
- Column A: "Time" or "Timestamp"
- Columns B, C, D, etc.: Channel names (e.g., "Throttle", "Steering", "Speed", "RPM")

### Row 2: Units (Optional)
- Column A: "s" (seconds) or leave empty
- Columns B, C, D, etc.: Units for each channel (e.g., "%", "km/h", "RPM")
- If Row 2 contains numeric data, it will be treated as the first data row

### Row 3+: Data Rows
- Column A: Timestamp in seconds (e.g., 0.0, 0.1, 0.2, ...)
- Columns B, C, D, etc.: Numeric values for each channel

## Example

```
| Time | Throttle | Steering | Speed | RPM  |
|------|----------|----------|-------|------|
| s    | %        | %        | km/h  | RPM  |
| 0.0  | 0        | 0        | 0     | 1000 |
| 0.1  | 25       | -10      | 5     | 2000 |
| 0.2  | 50       | -15      | 10    | 3000 |
| 0.3  | 75       | -5       | 15    | 4000 |
| 0.4  | 100      | 0        | 20    | 5000 |
```

## Tips

1. **Timestamp Column**: Must be in the first column (Column A)
2. **Numeric Data**: All data values must be numeric
3. **Consistent Sampling**: For best results, use consistent time intervals
4. **Multiple Channels**: You can have as many channels as needed
5. **File Size**: The application can handle files with thousands of data points

## Creating Your Own Data File

1. Export your Futaba 10PX telemetry logs
2. Convert to Excel format (.xlsx)
3. Ensure the data follows the format above
4. Open the file in the Futaba Telemetry Trace application
5. Use the playback controls to visualize your data
6. Export to video if needed

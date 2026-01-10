using OfficeOpenXml;
using FutabaTelemetryTrace.Models;
using System.IO;
using System.Windows.Media;

namespace FutabaTelemetryTrace.Services;

/// <summary>
/// Service for reading telemetry data from Excel files
/// </summary>
public class ExcelReaderService
{
    private const int StartRow = 3;
    private const int MaxColumnSupport = 9;

    public ExcelReaderService()
    {
        // Set EPPlus license context
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    /// <summary>
    /// Reads telemetry data from an Excel file
    /// Expected format:
    /// - First row: Channel names
    /// - Second row (optional): Units
    /// - Data rows: Timestamp in first column, then channel values
    /// </summary>
    public TelemetryData ReadFromExcel(string filePath)
    {
        var telemetryData = new TelemetryData();

        using var package = new ExcelPackage(new FileInfo(filePath));
        var worksheet = package.Workbook.Worksheets[0];

        if (worksheet.Dimension == null)
        {
            throw new InvalidDataException("The Excel file is empty or invalid.");
        }

        var rowCount = worksheet.Dimension.Rows;
        var colCount = Math.Min(worksheet.Dimension.Columns, MaxColumnSupport);

        if (rowCount < 2 || colCount < 2)
        {
            throw new InvalidDataException("The Excel file must have at least 2 rows and 2 columns.");
        }

        // Read channel names from first row (skip first column which is timestamp)
        var channelNames = new List<string>();
        for (var col = 2; col <= colCount; col++)
        {
            var cellValue = worksheet.Cells[StartRow, col].Value;
            var channelName = cellValue?.ToString() ?? $"Channel {col - 1}";
            channelNames.Add(channelName);
        }

        // Check if second row contains units or data
        var hasUnitRow = false;
        var secondRowFirstValue = worksheet.Cells[StartRow + 1, 1].Value;
        if (secondRowFirstValue != null && !double.TryParse(secondRowFirstValue.ToString(), out _))
        {
            hasUnitRow = true;
        }

        // Create channels with colors
        var colors = GetChannelColors(channelNames.Count);
        for (var i = 0; i < channelNames.Count; i++)
        {
            var channel = new TelemetryChannel
            {
                Name = channelNames[i],
                Unit = hasUnitRow && worksheet.Cells[2, i + 2].Value != null 
                    ? worksheet.Cells[2, i + 2].Value.ToString() ?? "" 
                    : "",
                DisplayColor = colors[i]
            };
            telemetryData.Channels.Add(channel);
        }

        // Read data rows
        var dataStartRow = hasUnitRow ? StartRow + 2 : StartRow + 1;
        for (var row = dataStartRow; row <= rowCount; row++)
        {
            var timestampValue = worksheet.Cells[row, 1].Value;
            if (timestampValue == null) continue;

            if (!double.TryParse(timestampValue.ToString(), out var timestamp))
            {
                continue;
            }

            var dataPoint = new TelemetryDataPoint { Timestamp = timestamp / 1000.0d };

            for (var col = 2; col <= colCount; col++)
            {
                var cellValue = worksheet.Cells[row, col].Value;
                if (cellValue != null && double.TryParse(cellValue.ToString(), out var value))
                {
                    dataPoint.ChannelValues[channelNames[col - 2]] = value;
                }
            }

            telemetryData.DataPoints.Add(dataPoint);
        }

        // Calculate min/max values for each channel
        foreach (var channel in telemetryData.Channels)
        {
            var values = telemetryData.DataPoints
                .Where(dp => dp.ChannelValues.ContainsKey(channel.Name))
                .Select(dp => dp.ChannelValues[channel.Name])
                .ToList();

            if (values.Count > 0)
            {
                channel.MinValue = values.Min();
                channel.MaxValue = values.Max();
            }
        }

        return telemetryData;
    }

    private List<Color> GetChannelColors(int count)
    {
        var colors = new List<Color>
        {
            Color.FromRgb(0, 120, 215),   // Blue
            Color.FromRgb(232, 17, 35),   // Red
            Color.FromRgb(0, 153, 76),    // Green
            Color.FromRgb(255, 185, 0),   // Yellow
            Color.FromRgb(142, 68, 173),  // Purple
            Color.FromRgb(0, 183, 195),   // Cyan
            Color.FromRgb(255, 140, 0),   // Orange
            Color.FromRgb(132, 117, 69),  // Brown
        };

        // If we need more colors, generate them
        while (colors.Count < count)
        {
            var random = new Random(colors.Count);
            colors.Add(Color.FromRgb(
                (byte)random.Next(50, 255),
                (byte)random.Next(50, 255),
                (byte)random.Next(50, 255)
            ));
        }

        return colors.Take(count).ToList();
    }
}

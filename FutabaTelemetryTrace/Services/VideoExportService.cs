using FFMediaToolkit.Encoding;
using FFMediaToolkit.Graphics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FutabaTelemetryTrace.Services;

/// <summary>
/// Service for exporting chart animations to video.
/// 
/// IMPORTANT: This service requires FFmpeg binaries to be available on the system
/// or bundled with the application. The binaries should be in the application directory
/// or in the system PATH. Download FFmpeg from https://ffmpeg.org/
/// </summary>
public class VideoExportService
{
    /// <summary>
    /// Export chart frames to video
    /// </summary>
    public async Task ExportToVideoAsync(
        Func<int, Task<BitmapSource>> frameProvider,
        int totalFrames,
        int fps,
        string outputPath,
        int width,
        int height,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        await Task.Run(async () =>
        {
            var settings = new VideoEncoderSettings(width, height, fps, VideoCodec.H264)
            {
                EncoderPreset = EncoderPreset.Fast,
                CRF = 23
            };

            using var videoFile = MediaBuilder.CreateContainer(outputPath).WithVideo(settings).Create();

            for (var frame = 0; frame < totalFrames; frame++)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                // Get frame from provider
                BitmapSource? frameBitmap = null;
                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    frameBitmap = await frameProvider(frame);
                });

                if (frameBitmap != null)
                {
                    // Convert BitmapSource to ImageData
                    var imageData = ConvertBitmapSourceToImageData(frameBitmap);
                    videoFile.Video.AddFrame(imageData);
                }

                progress?.Report(frame + 1);
            }
        }, cancellationToken);
    }

    private ImageData ConvertBitmapSourceToImageData(BitmapSource bitmapSource)
    {
        // Ensure the bitmap is in a compatible format
        var formatConvertedBitmap = new FormatConvertedBitmap();
        formatConvertedBitmap.BeginInit();
        formatConvertedBitmap.Source = bitmapSource;
        formatConvertedBitmap.DestinationFormat = PixelFormats.Bgra32;
        formatConvertedBitmap.EndInit();

        var width = formatConvertedBitmap.PixelWidth;
        var height = formatConvertedBitmap.PixelHeight;
        var stride = width * 4; // 4 bytes per pixel for BGRA32

        var pixels = new byte[height * stride];
        formatConvertedBitmap.CopyPixels(pixels, stride, 0);

        // Convert to ImageData - create array copy to avoid lifetime issues
        var pixelsCopy = new byte[pixels.Length];
        Array.Copy(pixels, pixelsCopy, pixels.Length);
        var imageData = ImageData.FromArray(pixelsCopy, ImagePixelFormat.Bgra32, width, height);
        return imageData;
    }
}

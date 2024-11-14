using System.Diagnostics;

namespace TiredDoctorManhattan.Wasm.Pages;

public partial class Index
{
    private const string BackgroundImageLocation = "assets/background.png";
    private const string FontLocation = "assets/KMKDSPK_.ttf";

    [Parameter] public string? TextToRender { get; set; }

    private static string PageTitle => "Tired Doctor Manhattan";

    private string AltText => $"Doctor Manhattan sitting on a rock on Mars saying: \"I am tired of {TextToRender}.\"";

    private byte[]? ImageBytes { get; set; }

    private string? GetImageBase64 => ImageBytes is null
        ? null
        : Convert.ToBase64String(ImageBytes);

    private bool IsWorking { get; set; }

    private TimeSpan? GenerationTime { get; set; }

    private async Task GenerateImage()
    {
        IsWorking = true;
        GenerationTime = null;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var text = TiredManhattanGenerator.Clean(TextToRender);

            await using var backgroundStream = await HttpClient.GetStreamAsync(BackgroundImageLocation);
            await using var fontStream = await HttpClient.GetStreamAsync(FontLocation);

            ImageBytes = await TiredManhattanGenerator.GenerateBytes(
                backgroundStream, fontStream, text);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        finally
        {
            IsWorking = false;
            stopwatch.Stop();
            GenerationTime = stopwatch.Elapsed;
        }
    }
}

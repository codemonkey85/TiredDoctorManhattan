namespace TiredDoctorManhattan.Wasm.Pages;

public partial class Index : IDisposable
{
    private const string BackgroundImageLocation = "assets/background.png";
    private const string FontLocation = "assets/KMKDSPK_.ttf";

    [Parameter] public string? TextToRender { get; set; }

    private static string PageTitle => "Tired Doctor Manhattan";

    private string AltText => $"Doctor Manhattan sitting on a rock on Mars saying: \"I am tired of {TextToRender}.\"";

    private byte[]? ImageBytes { get; set; }

    private bool IsUpdateAvailable { get; set; } = false;

    private string? GetImageBase64 => ImageBytes is null
        ? null
        : Convert.ToBase64String(ImageBytes);

    private bool IsWorking { get; set; }

    private TimeSpan? GenerationTime { get; set; }

    protected override void OnInitialized() => UpdateAlertService.OnUpdateAvailable += ShowUpdateMessage;

    public void Dispose() => UpdateAlertService.OnUpdateAvailable -= ShowUpdateMessage;

    public void ShowUpdateMessage()
    {
        // Display the alert when an update is available
        IsUpdateAvailable = true;
        StateHasChanged();
    }

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

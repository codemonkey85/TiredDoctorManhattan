namespace TiredDoctorManhattan.Wasm.Pages;

public partial class Index : IDisposable
{
    private const string BackgroundImageLocation = "assets/background.png";
    private const string FontLocation = "assets/KMKDSPK_.ttf";

    [Parameter]
    public string? TextToRender { get; set; }

    private MudInput<string?>? TextInput;

    private static string PageTitle => "Tired Doctor Manhattan";

    private const string BaseAltText = "Doctor Manhattan sitting on a rock on Mars";

    private string AltText => $"{BaseAltText} saying: \"I am tired of {TextToRender}.\"";

    private byte[]? ImageBytes { get; set; }

    private bool IsUpdateAvailable { get; set; }

    private string? GetImageBase64 => ImageBytes is null
        ? null
        : Convert.ToBase64String(ImageBytes);

    private bool IsWorking { get; set; }

    private TimeSpan? GenerationTime { get; set; }

    public void Dispose() => UpdateAlertService.OnUpdateAvailable -= ShowUpdateMessage;

    protected override void OnInitialized() => UpdateAlertService.OnUpdateAvailable += ShowUpdateMessage;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await FocusTextInput();
        }
    }

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
            await FocusTextInput();
        }
    }

    private async Task FocusTextInput()
    {
        if (TextInput is not null)
        {
            await TextInput.FocusAsync();
        }
    }
}

namespace TiredDoctorManhattan.Wasm.Pages;

public partial class Index
{
    private static string PageTitle => "Tired Doctor Manhattan";
    private string TextToRender { get; set; } = default!;
    private byte[]? ImageBytes { get; set; }
    private string? GetImageBase64 => ImageBytes is null ? null : Convert.ToBase64String(ImageBytes);
    private bool IsWorking { get; set; } = false;

    private async Task GenerateImage()
    {
        IsWorking = true;
        try
        {
            var text = TiredManhattanGenerator.Clean(TextToRender);
            using var backgroundStream = await HttpClient.GetStreamAsync("assets/background.png");
            using var fontStream = await HttpClient.GetStreamAsync("assets/KMKDSPK_.ttf");
            ImageBytes = await TiredManhattanGenerator.GenerateBytes(backgroundStream, fontStream, text);
            //var image = await TiredManhattanGenerator.Generate(stream, text);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        finally
        {
            IsWorking = false;
        }
    }
}
namespace TiredDoctorManhattan.Wasm.Pages;

public partial class Index
{
    private string textToRender { get; set; } = default!;
    private byte[]? imageBytes { get; set; }
    private string? getImageBase64 => imageBytes is null ? null : Convert.ToBase64String(imageBytes);

    private async Task GenerateImage()
    {
        var text = TiredManhattanGenerator.Clean(textToRender);
        using var backgroundStream = await HttpClient.GetStreamAsync("assets/background.png");
        using var fontStream = await HttpClient.GetStreamAsync("assets/KMKDSPK_.ttf");
        imageBytes = await TiredManhattanGenerator.GenerateBytes(backgroundStream, fontStream, text);
        //var image = await TiredManhattanGenerator.Generate(stream, text);
    }
}

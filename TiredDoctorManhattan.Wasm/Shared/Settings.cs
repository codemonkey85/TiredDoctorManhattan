namespace TiredDoctorManhattan.Wasm.Shared;

public static class Settings
{
    private static int FontSize => 26; //px

    private static Font? font;

    public static Font GetFont(Stream fontStream)
    {
        if (font is not null)
        {
            return font;
        }

        FontCollection collection = new();
        var family = collection.Add(fontStream);
        return font = family.CreateFont(FontSize, FontStyle.Bold);
    }

    // return a new instance every time
    public static async Task<Image> GetBackground(Stream stream) =>
        await Image.LoadAsync(stream);

    public static PointF TextBoxOrigin { get; } = new(x: 824, y: 165); // px

    public static float TextPadding => 15; //px

    public static float BlackBorderThickness => 3;

    public static float WhiteBorderThickness => 5;

    public static Color ManhattanBlue { get; } = new(new Rgba32(1, 215, 253));
}

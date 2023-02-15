namespace TiredDoctorManhattan.Shared;

public static class Settings
{
    static Settings()
    {
        var config = new SlugHelperConfiguration();
        config.StringReplacements.Add(".", string.Empty);
        config.TrimWhitespace = true;
        config.ForceLowerCase = true;
        SlugHelper = new SlugHelper(config);
    }

    private static readonly SlugHelper SlugHelper;
    private static int FontSize => 26; //px

    private static Font? _font;
    public static Font GetFont(Stream fontStream)
    {
        if (_font is not null)
        {
            return _font;
        }
        FontCollection collection = new();
        var family = collection.Add(fontStream);
        return _font = family.CreateFont(FontSize, FontStyle.Bold);
    }

    // return a new instance every time
    public static async Task<Image> GetBackground(Stream stream)
        => await Image.LoadAsync(stream);

    public static PointF TextBoxOrigin { get; } = new(x: 824, y: 165); // px
    public static float TextPadding => 15; //px
    public static float BlackBorderThickness => 3;
    public static float WhiteBorderThickness => 5;
    public static Color ManhattanBlue { get; } = new(new Rgba32(1, 215, 253));

    public static string Slugify(this string value) => SlugHelper.GenerateSlug(value);
}

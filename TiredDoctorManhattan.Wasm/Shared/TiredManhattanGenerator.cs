namespace TiredDoctorManhattan.Wasm.Shared;

public static class TiredManhattanGenerator
{
    [SuppressMessage("Style", "IDE0058:Expression value is never used", Justification = "<Pending>")]
    private static async Task<Image> Generate(
        Stream backgroundStream,
        Stream fontStream,
        string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var background = await Settings.GetBackground(backgroundStream);

        var textOptions = new RichTextOptions(Settings.GetFont(fontStream))
        {
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            Origin = Settings.TextBoxOrigin
        };

        var textRectangle = TextMeasurer.MeasureBounds(text, textOptions);

        var width = textRectangle.Width + Settings.TextPadding * 2;
        var height = textRectangle.Height + Settings.TextPadding * 2;

        var container = new RectangularPolygon(
            x: Settings.TextBoxOrigin.X - Settings.TextPadding,
            y: Settings.TextBoxOrigin.Y - height / 2,
            width: width,
            height: height
        );
        var blackBorder = new RectangularPolygon(
            x: container.X - Settings.BlackBorderThickness,
            y: container.Y - Settings.BlackBorderThickness,
            width: container.Width + Settings.BlackBorderThickness * 2,
            height: container.Height + Settings.BlackBorderThickness * 2
        );
        var whiteBorder = new RectangularPolygon(
            x: blackBorder.X - Settings.WhiteBorderThickness,
            y: blackBorder.Y - Settings.WhiteBorderThickness,
            width: blackBorder.Width + Settings.WhiteBorderThickness * 2,
            height: blackBorder.Height + Settings.WhiteBorderThickness * 2
        );

        background.Mutate(i =>
        {
            i.Fill(Color.White, whiteBorder);
            i.Fill(Color.Black, blackBorder);
            i.Fill(Settings.ManhattanBlue, container);
            i.DrawText(textOptions, text, Color.Black);
            // resize the image, it's kind of big right now
            i.Resize(background.Width / 2, background.Height / 2);
        });

        return background;
    }

    [SuppressMessage("Style", "IDE0058:Expression value is never used", Justification = "<Pending>")]
    public static async Task<byte[]> GenerateBytes(
        Stream backgroundStream,
        Stream fontStream,
        string text)
    {
        using var ms = new MemoryStream();
        var image = await Generate(backgroundStream, fontStream, text);

        await image.SaveAsync(ms, PngFormat.Instance);
        ms.Position = 0;
        ms.Seek(0, SeekOrigin.Begin);

        return ms.ToArray();
    }

    public static string Clean(string? text)
    {
        var content = text?.Trim() switch
        {
            null or { Length: 0 } => "THE EMPTINESS",
            var t when t.Length > 30 => "LONG TEXT",
            var t => t.ToUpperInvariant()
        };

        return $"I AM TIRED OF {content}.";
    }
}

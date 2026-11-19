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
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(text.Length, 0);

        var background = await Settings.GetBackground(backgroundStream);

        // Yield to prevent blocking the UI thread
        await Task.Yield();

        var textOptions = new RichTextOptions(Settings.GetFont(fontStream))
        {
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            Origin = Settings.TextBoxOrigin
        };

        var textRectangle = TextMeasurer.MeasureBounds(text, textOptions);

        var width = textRectangle.Width + Settings.TextPadding * 2;
        var height = textRectangle.Height + Settings.TextPadding * 2;

        var container = new RectanglePolygon(
            Settings.TextBoxOrigin.X - Settings.TextPadding,
            Settings.TextBoxOrigin.Y - height / 2,
            width,
            height
        );

        var blackBorder = new RectanglePolygon(
            container.X - Settings.BlackBorderThickness,
            container.Y - Settings.BlackBorderThickness,
            container.Width + Settings.BlackBorderThickness * 2,
            container.Height + Settings.BlackBorderThickness * 2
        );

        var whiteBorder = new RectanglePolygon(
            blackBorder.X - Settings.WhiteBorderThickness,
            blackBorder.Y - Settings.WhiteBorderThickness,
            blackBorder.Width + Settings.WhiteBorderThickness * 2,
            blackBorder.Height + Settings.WhiteBorderThickness * 2
        );

        // Yield before the CPU-intensive image manipulation
        await Task.Yield();

        background.Mutate(i =>
        {
            i.Paint(canvas =>
            {
                canvas.Fill(Brushes.Solid(ImageSharpColor.White), whiteBorder);
                canvas.Fill(Brushes.Solid(ImageSharpColor.Black), blackBorder);
                canvas.Fill(Brushes.Solid(Settings.ManhattanBlue), container);
                canvas.DrawText(textOptions, text, Brushes.Solid(ImageSharpColor.Black), pen: null);
            });
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
        using var image = await Generate(backgroundStream, fontStream, text);

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

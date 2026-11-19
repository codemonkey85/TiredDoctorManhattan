namespace TiredDoctorManhattan.Tests;

public class TiredManhattanGeneratorTests
{
    [Theory]
    [InlineData(null, "I AM TIRED OF THE EMPTINESS.")]
    [InlineData("", "I AM TIRED OF THE EMPTINESS.")]
    [InlineData("   ", "I AM TIRED OF THE EMPTINESS.")]
    [InlineData("  waiting  ", "I AM TIRED OF WAITING.")]
    [InlineData("123456789012345678901234567890", "I AM TIRED OF 123456789012345678901234567890.")]
    [InlineData("1234567890123456789012345678901", "I AM TIRED OF LONG TEXT.")]
    public void CleanNormalizesInput(string? input, string expected) =>
        Assert.Equal(expected, TiredManhattanGenerator.Clean(input));

    [Theory]
    [InlineData("waiting")]
    [InlineData("")]
    [InlineData("123456789012345678901234567890")]
    public async Task GenerateBytesRendersTextAndBordersIntoHalfSizePng(string input)
    {
        await using var background = File.OpenRead(AssetPath("background.png"));
        await using var font = File.OpenRead(AssetPath("KMKDSPK_.ttf"));
        var bytes = await TiredManhattanGenerator.GenerateBytes(background, font, TiredManhattanGenerator.Clean(input));

        Assert.Equal(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, bytes[..8]);
        using var image = Image.Load<Rgba32>(bytes);
        using var original = Image.Load<Rgba32>(AssetPath("background.png"));
        Assert.Equal(original.Width / 2, image.Width);
        Assert.Equal(original.Height / 2, image.Height);
        original.Mutate(context => context.Resize(image.Width, image.Height));
        Assert.Equal(original[20, 20], image[20, 20]);

        // The box starts just left of the text origin. Its left edge contains
        // the white outer border, black inner border, then the blue fill.
        var centerY = (int)Settings.TextBoxOrigin.Y / 2;
        var leftX = (int)Settings.TextBoxOrigin.X / 2;
        Assert.Equal(ImageSharpColor.White.ToPixel<Rgba32>(), image[leftX - 11, centerY]);
        var borderPixel = image[leftX - 9, centerY];
        Assert.True(borderPixel.R < 30 && borderPixel.G < 30 && borderPixel.B < 30,
            "Expected a black inner border after downsampling.");
        Assert.Equal(Settings.ManhattanBlue.ToPixel<Rgba32>(), image[leftX - 5, centerY]);

        // Check the glyph area separately from the borders so a blank blue
        // box cannot satisfy the rendering regression test.
        var blackPixels = 0;
        for (var y = centerY - 5; y <= centerY + 5; y++)
        {
            for (var x = leftX + 2; x < leftX + 80; x++)
            {
                var pixel = image[x, y];
                if (pixel.R < 50 && pixel.G < 50 && pixel.B < 50)
                {
                    blackPixels++;
                }
            }
        }

        Assert.True(blackPixels > 30, "Expected visible black text inside the blue box.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task GenerateBytesRejectsMissingText(string? text)
    {
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            TiredManhattanGenerator.GenerateBytes(Stream.Null, Stream.Null, text!));
    }

    [Fact]
    public async Task DifferentTextProducesDifferentImages()
    {
        async Task<byte[]> Generate(string text)
        {
            await using var background = File.OpenRead(AssetPath("background.png"));
            await using var font = File.OpenRead(AssetPath("KMKDSPK_.ttf"));
            return await TiredManhattanGenerator.GenerateBytes(background, font, TiredManhattanGenerator.Clean(text));
        }

        var first = await Generate("waiting");
        var second = await Generate("testing");
        Assert.False(first.SequenceEqual(second));
    }

    private static string AssetPath(string name) => System.IO.Path.Combine(AppContext.BaseDirectory, "assets", name);
}

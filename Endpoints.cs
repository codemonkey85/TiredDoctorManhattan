namespace TiredDoctorManhattan;

public static class Endpoints
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/",
            () => Results.Content( /* language=html */
                "<html lang='en'><h1><a href='https://twitter.com/TiredManhattan'>@TiredManhattan</a></h1> <ul><li><a href='/image?text=.NET'>.NET</a></li> <li><a href='/image?text=Programming'>Programming</a></li> <li><a href='/image?text=Vegetables'>Vegetables</a></li></ul></html>",
                "text/html"));

        app.MapGet("/image", async (string? text) =>
        {
            text = TiredManhattanGenerator.Clean(text);
            var image = await TiredManhattanGenerator.GenerateBytes(text);
            return Results.File(image, "image/png");
        });

        app.MapPost("/tweet",
            async (string text, IProfanityFilter profanityFilter, TwitterClient twitter, ILogger<Program> logger) =>
            {
                logger.LogInformation("{Text}", text);

                // I'm not dealing with this s#@$!
                if (profanityFilter.IsProfanity(text))
                {
                    logger.LogInformation("Filtered out {Text}", text);
                    return;
                }

                try
                {
                    var content = TiredManhattanGenerator.Clean(text);
                    var image = await TiredManhattanGenerator.GenerateBytes(content);
                    var upload = await twitter.Upload.UploadTweetImageAsync(image);

                    var parameters = new PublishTweetParameters
                    {
                        Medias = { upload }
                    };

                    await twitter.Tweets.PublishTweetAsync(parameters);
                    logger.LogInformation("Sent to {Text}", text);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Unable to reply to {Text}", text);
                }
            });

        return app;
    }
}

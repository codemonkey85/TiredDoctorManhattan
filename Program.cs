var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// tired dr. manhattan
var credentials = new TwitterCredentials
{
    ConsumerKey = configuration["API_KEY"],
    ConsumerSecret = configuration["API_KEY_SECRET"],
    AccessToken = configuration["ACCESS_TOKEN"],
    AccessTokenSecret = configuration["ACCESS_TOKEN_SECRET"],
};

// get text and measure it for box.
var twitterClient = new TwitterClient(credentials);

await twitterClient.Auth.InitializeClientBearerTokenAsync();
var user = await twitterClient.Users.GetAuthenticatedUserAsync();

builder.Services.AddSingleton(twitterClient);
builder.Services.AddSingleton(new UserInfo(user.Id, user.ScreenName));
builder.Services.AddSingleton<IProfanityFilter>(new ProfanityFilter.ProfanityFilter());

//builder.Services.AddHostedService<DrManhattanResponder>();

var app = builder.Build();

app.MapEndpoints();

await app.RunAsync();

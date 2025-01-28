var builder = WebAssemblyHostBuilder.CreateDefault(args);
var services = builder.Services;

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

services
    .AddSingleton(_ => new HttpClient { BaseAddress = new(builder.HostEnvironment.BaseAddress) })
    .AddSingleton<IUpdateAlertService, UpdateAlertService>()
    .AddMudServices();

await builder.Build().RunAsync();

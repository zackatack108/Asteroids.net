using Asteroids.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

string signalRHubUrl = builder.Configuration.GetSection("SIGNALR_HUB_URL").Value ?? "";

if (signalRHubUrl == null || signalRHubUrl == "${SIGNALR_HUB_URL}") signalRHubUrl = "http://localhost:5000/asteroidHub";

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddSingleton<SignalRHandler>(provider =>
{
    return new SignalRHandler(signalRHubUrl);
});

await builder.Build().RunAsync();


using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Tribulus.Adzup.Player.Blazor;
using Tribulus.Adzup.Player.Shared.IO;
using Tribulus.Adzup.Player.Shared.Service;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<PlayerService>();
builder.Services.AddSingleton<Storage>();

await builder.Build().RunAsync();

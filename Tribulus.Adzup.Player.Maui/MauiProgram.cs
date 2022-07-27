using CommunityToolkit.Maui;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Tribulus.Adzup.Player.Maui.PeriodicTask;
using Tribulus.Adzup.Player.Maui.View;
using Tribulus.Adzup.Player.Maui.ViewModel;
using Tribulus.Adzup.Player.Shared.IO;
using Tribulus.Adzup.Player.Shared.Service;

namespace Tribulus.Adzup.Player.Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp(true)
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder.Services.AddSingleton<PlayerView>();
            builder.Services.AddSingleton<PlayerViewModel>();
            builder.Services.AddSingleton<PlayerService>();
            builder.Services.AddSingleton<PlayerTask>();
            builder.Services.AddSingleton<Storage>();

            

            return builder.Build();
        }
    }
}
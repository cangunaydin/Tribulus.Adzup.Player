using CommunityToolkit.Maui;
using Tribulus.Adzup.Player.Maui.Service;
using Tribulus.Adzup.Player.Maui.View;
using Tribulus.Adzup.Player.Maui.ViewModel;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Tribulus.Adzup.Player.FFmpeg;
using Tribulus.Adzup.Player.Maui.PeriodicTasks;

namespace Tribulus.Adzup.Player.Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
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

            

            return builder.Build();
        }
    }
}
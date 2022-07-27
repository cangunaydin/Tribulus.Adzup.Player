using Tribulus.Adzup.Player.Maui.Controls;
using Tribulus.Adzup.Player.Maui.PeriodicTask;
using Tribulus.Adzup.Player.Maui.ViewModel;

namespace Tribulus.Adzup.Player.Maui
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            MainPage = new AppShell();
            _serviceProvider = serviceProvider;
        }
        protected override async void OnSleep()
        {
            base.OnSleep();
            var playerTask=_serviceProvider.GetService<PlayerTask>();
            var playerViewModel = _serviceProvider.GetService<PlayerViewModel>();
            await playerTask.StopAsync();
            playerViewModel.Stop = true;
            playerViewModel.Play = false;

        }
        protected override void OnResume()
        {
            base.OnResume();
            var playerTask = _serviceProvider.GetService<PlayerTask>();
            var playerViewModel = _serviceProvider.GetService<PlayerViewModel>();
            playerTask.Start();
            playerViewModel.Stop = false;
            playerViewModel.Play = true;
            
        }
        protected override void OnStart()
        {
            base.OnStart();
        }
        
    }
}
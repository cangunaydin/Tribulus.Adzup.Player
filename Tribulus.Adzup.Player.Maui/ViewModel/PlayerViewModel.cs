using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tribulus.Adzup.Player.FFmpeg;
using Tribulus.Adzup.Player.Maui.Model;
using Tribulus.Adzup.Player.Maui.PeriodicTasks;
using Tribulus.Adzup.Player.Maui.Service;

namespace Tribulus.Adzup.Player.Maui.ViewModel
{
    public partial class PlayerViewModel : BaseViewModel
    {
        [ObservableProperty]
        private List<PlaylistFile> _playlistFiles = new();

        [ObservableProperty]
        private bool _play = false;

        private PlayerService _playerService;
        private PlayerTask _playerTask;
        public PlayerViewModel(PlayerService playerService,PlayerTask playerTask)
        {
            _playerService = playerService;
            _playerTask = playerTask;
            MessagingCenter.Subscribe<PlayerTask, List<PlaylistFile>>(this, PlayerTask.MessagingKey, async (sender, playlistFiles) =>
            {
                await CreateNewPlaylist(playlistFiles);
            });
            _playerTask.Start();
        }

        private async Task CreateNewPlaylist(List<PlaylistFile> playlistFiles)
        {
            Play = false;
            await Task.Delay(1000);
            string appDataDir = FileSystem.Current.AppDataDirectory;
            foreach (var playlistFile in playlistFiles)
            {
                string filePath = Path.Combine(appDataDir, playlistFile.Name);
                playlistFile.Path = filePath;
                await _playerService.DownloadFileToAsync(playlistFile.DownloadUrl, playlistFile.Path);
            }
            PlaylistFiles = playlistFiles;
            Play = true;
        }

        [RelayCommand]
        async Task PageAppearingAsync()
        {
           
        }
        [RelayCommand]
        async Task PageDisappearingAsync()
        {
            Play = false;
            await _playerTask.StopAsync();
            MessagingCenter.Unsubscribe<PlayerViewModel, List<PlaylistFile>>(this, PlayerTask.MessagingKey);
        }
    }
}

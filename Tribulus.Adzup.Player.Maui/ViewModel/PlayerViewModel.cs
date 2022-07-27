using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tribulus.Adzup.Player.Maui.PeriodicTask;
using Tribulus.Adzup.Player.Shared.Service;

namespace Tribulus.Adzup.Player.Maui.ViewModel;

public partial class PlayerViewModel : BaseViewModel,IDisposable
{
    [ObservableProperty]
    private List<PlaylistFile> _playlistFiles = new();

    [ObservableProperty]
    private bool _play = false;

    [ObservableProperty]
    private bool _stop = false;

    private PlayerTask _playerTask;
    public PlayerViewModel(PlayerTask playerTask)
    {
        _playerTask = playerTask;
        _playerTask.OnPlaylistChanged += ChangePlaylist;
      
        _playerTask.Start();
    }

   

    private void ChangePlaylist(List<PlaylistFile> playlistFiles)
    {
        Play = false;
        PlaylistFiles = playlistFiles;
        Play = true;
    }

    [RelayCommand]
    async Task PageAppearingAsync()
    {
       await _playerTask.DoWorkAsync();
    }
    [RelayCommand]
    async Task PageDisappearingAsync()
    {
        await _playerTask.StopAsync();
        
    }
    public void Dispose()
    {
        _playerTask.OnPlaylistChanged -= ChangePlaylist;
    }

}

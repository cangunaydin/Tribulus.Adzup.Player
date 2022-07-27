using Tribulus.Adzup.Player.Shared.Extension;
using Tribulus.Adzup.Player.Shared.IO;
using Tribulus.Adzup.Player.Shared.Model;
using Tribulus.Adzup.Player.Shared.PeriodicTask;
using Tribulus.Adzup.Player.Shared.Service;

namespace Tribulus.Adzup.Player.Maui.PeriodicTask;

public class PlayerTask : BackgroundTask
{
    public static string MessagingKey = "PlaylistChanged";
    public event Action<List<PlaylistFile>> OnPlaylistChanged;

    private const int _timeInterval = 30000; //ms
    private readonly PlayerService _playerService;
    private readonly Storage _storage;
    private List<PlaylistFile> _playlistFiles = new();
    public PlayerTask(PlayerService playerService,Storage storage) : base(TimeSpan.FromMilliseconds(_timeInterval))
    {
        _playerService = playerService;
        _storage = storage;
    }

    public override async Task DoWorkAsync()
    {
        var latestPlaylistFiles = await _playerService.GetPlaylistFilesAsync();
        if (_playlistFiles.IsPlaylistChanged(latestPlaylistFiles))
        {
            string appDataDir = FileSystem.Current.AppDataDirectory;

            var filesDirPath = _storage.CreateDirectory(appDataDir);

            await _storage.CreateNewPlaylistFiles(latestPlaylistFiles, filesDirPath);
            //MessagingCenter.Send(this, MessagingKey, latestPlaylistFiles);
            NotifyStateChanged(latestPlaylistFiles);
            //delete files that is not necessary anymore after some time.
            await Task.Delay(1000);
            var deletedFiles = _playlistFiles.Where(o => latestPlaylistFiles.Any(x => x.Path != o.Path)).ToList();
            _storage.DeleteFiles(deletedFiles);
            _playlistFiles = latestPlaylistFiles;
        }
    }
    private void NotifyStateChanged(List<PlaylistFile> playlistFiles) => OnPlaylistChanged?.Invoke(playlistFiles);





}

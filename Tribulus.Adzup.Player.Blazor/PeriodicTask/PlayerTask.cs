using Tribulus.Adzup.Player.FFmpeg;
using Tribulus.Adzup.Player.Shared.Extension;
using Tribulus.Adzup.Player.Shared.IO;
using Tribulus.Adzup.Player.Shared.Model;
using Tribulus.Adzup.Player.Shared.PeriodicTask;
using Tribulus.Adzup.Player.Shared.Service;

namespace Tribulus.Adzup.Player.Blazor.PeriodicTask
{
    public class PlayerTask : BackgroundTask
    {
        public event Action<List<PlaylistFile>>? OnPlaylistChanged;

        private const int _timeInterval = 30000; //ms
        private readonly PlayerService _playerService;
        private List<PlaylistFile> _playlistFiles = new();
        public PlayerTask(PlayerService playerService, Storage storage) : base(TimeSpan.FromMilliseconds(_timeInterval))
        {
            _playerService = playerService;
        }

        public override async Task DoWorkAsync()
        {
            var latestPlaylistFiles = await _playerService.GetPlaylistFilesAsync();
            if (_playlistFiles.IsPlaylistChanged(latestPlaylistFiles))
            {
                foreach (var playlistFile in latestPlaylistFiles)
                {

                    var fileBytes = await _playerService.DownloadFileAsBytesToAsync(playlistFile.DownloadUrl);
                    if (playlistFile.Type == PlaylistFileType.Video)
                    {
                        var ffmpegVideoReader = new VideoReader(fileBytes, playlistFile.Name);
                        playlistFile.Path = ffmpegVideoReader.Filepath;
                    }
                    else
                    {
                        playlistFile.FileBytes = fileBytes;
                    }

                }
                NotifyStateChanged(latestPlaylistFiles);
                //delete files that is not necessary anymore after some time.

                await Task.Delay(1000);
                var deletedFiles = _playlistFiles.Where(o => latestPlaylistFiles.Any(x => x.Path != o.Path)).ToList();

                foreach (var deletedFile in deletedFiles)
                {
                    if (deletedFile.Type != PlaylistFileType.Video)
                        continue;
                    VideoReader videoReader = new VideoReader();
                    videoReader.DeleteFile(deletedFile.Path);
                }
                _playlistFiles = latestPlaylistFiles;
            }
        }
        private void NotifyStateChanged(List<PlaylistFile> playlistFiles) => OnPlaylistChanged?.Invoke(playlistFiles);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tribulus.Adzup.Player.Maui.Model;
using Tribulus.Adzup.Player.Maui.Service;

namespace Tribulus.Adzup.Player.Maui.PeriodicTasks
{
    public class PlayerTask : BackgroundTask
    {
        public static string MessagingKey = "PlaylistChanged";

        private const int _timeInterval = 30000; //ms
        private readonly PlayerService _playerService;
        private List<PlaylistFile> _playlistFiles=new();
        public PlayerTask(PlayerService playerService) : base(TimeSpan.FromMilliseconds(_timeInterval))
        {
            _playerService= playerService;
        }

        public override async Task DoWorkAsync()
        {
            var playlistFiles = await _playerService.GetPlaylistFilesAsync();
            var isPlaylistChanged=IsPlaylistChanged(playlistFiles);
            if (isPlaylistChanged)
            {
                MessagingCenter.Send<PlayerTask, List<PlaylistFile>>(this,MessagingKey, playlistFiles);
                _playlistFiles = playlistFiles;
            }
        }
        public bool IsPlaylistChanged(List<PlaylistFile> playlistFiles)
        {
            if (_playlistFiles.Count != playlistFiles.Count )
                return true;

            var isPlaylistChanged = false;
            foreach (var playlistFile in playlistFiles)
            {
                bool isExist=_playlistFiles.Any(pf => playlistFile.DownloadUrl == pf.DownloadUrl);
                if (!isExist)
                {
                    isPlaylistChanged = true;
                    break;
                }
            }
            return isPlaylistChanged;
        }
    }
}

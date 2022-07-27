using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tribulus.Adzup.Player.Shared.Model;

namespace Tribulus.Adzup.Player.Shared.Extension
{
    public static class PlaylistFileExtension
    {
        public static bool  IsPlaylistChanged(this List<PlaylistFile> playlistFiles,List<PlaylistFile> newPlaylistFiles)
        {
            //if (newPlaylistFiles.Count != playlistFiles.Count)
            //    return true;

            //var isPlaylistChanged = false;
            //foreach (var playlistFile in playlistFiles)
            //{
            //    bool isExist = newPlaylistFiles.Any(pf => playlistFile.DownloadUrl == pf.DownloadUrl);
            //    if (!isExist)
            //    {
            //        isPlaylistChanged = true;
            //        break;
            //    }
            //}
            //return isPlaylistChanged;
            return !playlistFiles.SequenceEqual(newPlaylistFiles);
        }
    }
}

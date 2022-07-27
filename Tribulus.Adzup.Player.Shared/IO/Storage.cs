using Tribulus.Adzup.Player.Shared.Model;
using Tribulus.Adzup.Player.Shared.Service;

namespace Tribulus.Adzup.Player.Shared.IO;

public class Storage
{
    public static string FilesDirectoryPath = "Tribulus_Adzup_Player_Files";
    private readonly PlayerService _playerService;
    public Storage(PlayerService playerService)
    {
        _playerService = playerService;
    }
    public async Task CreateNewPlaylistFiles(List<PlaylistFile> playlistFiles, string filesDirPath)
    {
        foreach (var playlistFile in playlistFiles)
        {
            string filePath = Path.Combine(filesDirPath, playlistFile.Name);
            await _playerService.DownloadFileToAsync(playlistFile.DownloadUrl, filePath);
            playlistFile.Path = filePath;
        }
    }
    public string CreateDirectory(string rootDirectoryPath)
    {
        string fullDirPath = Path.Combine(rootDirectoryPath, FilesDirectoryPath);
        Directory.CreateDirectory(fullDirPath);
        return fullDirPath;
    }
    public void DeleteFiles(List<PlaylistFile> deletedPlaylistFiles)
    {
        foreach (var deletedPlaylistFile in deletedPlaylistFiles)
        {
            File.Delete(deletedPlaylistFile.Path);
        }
    }
}

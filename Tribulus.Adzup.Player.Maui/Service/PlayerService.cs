using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Tribulus.Adzup.Player.Maui.Model;

namespace Tribulus.Adzup.Player.Maui.Service
{
    public class PlayerService
    {
        private HttpClient _httpClient;
        private List<PlaylistFile> _playlistFiles;

        public PlayerService()
        {
            _httpClient = new HttpClient();
        }
        public async Task<List<PlaylistFile>> GetPlaylistFilesAsync()
        {

            var url = "https://raw.githubusercontent.com/cangunaydin/maui_test/main/playlist.json?d="+DateTime.Now.Ticks.ToString();
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Can not get files, status code:"+response.StatusCode);
            }
            _playlistFiles = await response.Content.ReadFromJsonAsync<List<PlaylistFile>>();

            return _playlistFiles;

        }
        public async Task<byte[]> DownloadFileAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Can not download the file" + response.StatusCode);
            }
            return await response.Content.ReadAsByteArrayAsync();
        }
        public async Task DownloadFileToAsync(string url,string filePath)
        {


            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Can not download the file" + response.StatusCode);
            }
            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                await response.Content.CopyToAsync(fs);
            }
                
            
        }
    }
}

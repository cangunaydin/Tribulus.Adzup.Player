using System.Net.Http.Json;
using Tribulus.Adzup.Player.Shared.Model;

namespace Tribulus.Adzup.Player.Shared.Service
{
    public class PlayerService
    {
        private HttpClient _httpClient;

        public PlayerService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");


        }
        public async Task<List<PlaylistFile>> GetPlaylistFilesAsync()
        {
            //var url = "https://raw.githubusercontent.com/cangunaydin/maui_test/main/playlist.json?cache=" + Guid.NewGuid().ToString();
            var url = "http://192.168.0.17:4444/playlist.json?cache=" + Guid.NewGuid().ToString();
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Can not get files, status code:" + response.StatusCode);
            }
            return await response.Content.ReadFromJsonAsync<List<PlaylistFile>>() ?? new List<PlaylistFile>();
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
        public async Task DownloadFileToAsync(string url, string filePath)
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

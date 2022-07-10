using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tribulus.Adzup.Player.Maui.Model
{
    public class PlaylistFile
    {
        public string Name { get; set; }
        
        

        public string DownloadUrl { get; set; }
        public int Duration { get; set; } //in ms

        public PlaylistFileType Type { get; set; }

        public byte[] FileBytes { get; set; }

        public string Path { get; set; }
    }
}

namespace Tribulus.Adzup.Player.Shared.Model; 

public class PlaylistFile:IEquatable<PlaylistFile>
{
    public string Name { get; set; }

    public string DownloadUrl { get; set; }
    public int Duration { get; set; } //in ms

    public PlaylistFileType Type { get; set; }

    public byte[] FileBytes { get; set; }

    public string Path { get; set; }

    public bool Equals(PlaylistFile? other)
    {
        if (other is null)
            return false;

        return this.Name == other.Name && this.DownloadUrl == other.DownloadUrl && this.Duration == other.Duration && this.Type == other.Type;
    }
    public override bool Equals(object obj) => Equals(obj as PlaylistFile);
    public override int GetHashCode() => (Name, DownloadUrl,Duration,Type).GetHashCode();
}

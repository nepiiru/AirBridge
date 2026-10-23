namespace AirBridge.Core.Models;

public sealed class NowPlaying
{
    public string? Title { get; set; }
    public string? Artist { get; set; }
    public string? Album { get; set; }
    public byte[]? Artwork { get; set; }
    public double PositionSeconds { get; set; }
    public double DurationSeconds { get; set; }

    public bool HasMetadata =>
        !string.IsNullOrWhiteSpace(Title) ||
        !string.IsNullOrWhiteSpace(Artist) ||
        !string.IsNullOrWhiteSpace(Album);

    public void Clear()
    {
        Title = null;
        Artist = null;
        Album = null;
        Artwork = null;
        PositionSeconds = 0;
        DurationSeconds = 0;
    }
}

public sealed class AudioDevice
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public bool IsDefault { get; init; }

    public override string ToString() => Name;
}

public sealed class ClientInfo
{
    public string? Name { get; set; }
    public string? Model { get; set; }
    public string? DeviceId { get; set; }
}

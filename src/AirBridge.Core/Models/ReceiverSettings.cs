namespace AirBridge.Core.Models;

/// <summary>Access control mode exposed by the native engine.</summary>
public enum PinMode
{
    None = 0,
    RandomPin = 1,
    Password = 2,
}

/// <summary>High-level state reported by the AirPlay engine.</summary>
public enum PlaybackState
{
    Idle,
    Connected,
    Audio,
    Mirroring,
    Hls,
}

public sealed class ReceiverSettings
{
    public string ReceiverName { get; set; } = "AirBridge";
    public bool AppendHostname { get; set; } = true;
    public int Port { get; set; } = 7000;
    public string? AudioDeviceId { get; set; }
    public string VideoSink { get; set; } = "d3d11videosink";
    public bool HlsEnabled { get; set; } = true;
    public PinMode PinMode { get; set; } = PinMode.None;
    public string? Password { get; set; }
    public bool NoHold { get; set; }
    public bool AutoStart { get; set; } = true;
    public double VolumeDb { get; set; } = -15.0;
}

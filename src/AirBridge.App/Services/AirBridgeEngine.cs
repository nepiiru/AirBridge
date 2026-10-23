using System.Runtime.InteropServices;
using AirBridge.App.Interop;
using AirBridge.Core.Models;

namespace AirBridge.App.Services;

/// <summary>
/// Managed wrapper around airbridge_core.dll. Native callbacks arrive on
/// arbitrary threads; consumers must marshal to the UI thread themselves.
/// </summary>
public sealed class AirBridgeEngine : IDisposable
{
    private NativeApi.AirBridgeCallbacks _callbacks;
    private bool _initialized;
    private bool _disposed;

    public event Action<PlaybackState>? StateChanged;
    public event Action<ClientInfo>? ClientConnected;
    public event Action<NowPlaying>? MetadataChanged;
    public event Action<byte[]>? ArtworkChanged;
    public event Action<double, double>? ProgressChanged;
    public event Action<double, double>? VolumeChanged;
    public event Action<string>? PinRequired;
    public event Action<int, int>? VideoSizeChanged;
    public event Action<int, string>? ErrorOccurred;
    public event Action<int, string>? LogReceived;

    public AirBridgeEngine()
    {
        _callbacks = new NativeApi.AirBridgeCallbacks
        {
            UserData = IntPtr.Zero,
            OnState = HandleState,
            OnClient = HandleClient,
            OnMetadata = HandleMetadata,
            OnArtwork = HandleArtwork,
            OnProgress = HandleProgress,
            OnVolume = HandleVolume,
            OnPin = HandlePin,
            OnVideoSize = HandleVideoSize,
            OnError = HandleError,
            OnLog = HandleLog,
        };
    }

    public static string Version
    {
        get
        {
            var ptr = NativeApi.airbridge_version();
            return ptr == IntPtr.Zero ? "unknown" : Marshal.PtrToStringUTF8(ptr) ?? "unknown";
        }
    }

    public void Initialize(ReceiverSettings settings)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var config = new NativeApi.AirBridgeConfig
        {
            ReceiverName = settings.ReceiverName,
            AppendHostname = settings.AppendHostname ? 1 : 0,
            AudioDevice = settings.AudioDeviceId,
            VideoSink = settings.VideoSink,
            Port = settings.Port,
            HlsEnabled = settings.HlsEnabled ? 1 : 0,
            PinMode = (int)settings.PinMode,
            Pin = settings.Password,
            NoHold = settings.NoHold ? 1 : 0,
            LogLevel = 1,
        };

        var result = NativeApi.airbridge_init(ref config, ref _callbacks);
        NativeLog.Write($"airbridge_init result={result} name={settings.ReceiverName} appendHostname={settings.AppendHostname} port={settings.Port}");
        if (result != 0)
        {
            throw new InvalidOperationException($"airbridge_init failed (code {result}).");
        }

        _initialized = true;
    }

    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!_initialized)
        {
            throw new InvalidOperationException("Engine has not been initialized.");
        }

        var result = NativeApi.airbridge_start();
        NativeLog.Write($"airbridge_start result={result}");
        if (result != 0)
        {
            throw new InvalidOperationException($"airbridge_start failed (code {result}).");
        }
    }

    public void SetWindowHandle(IntPtr hwnd)
    {
        if (_disposed)
        {
            return;
        }
        NativeApi.airbridge_set_window_handle(hwnd);
    }

    public void SetVolume(double db)
    {
        if (_disposed)
        {
            return;
        }
        NativeApi.airbridge_set_volume(db);
    }

    public void Stop()
    {
        if (_disposed)
        {
            return;
        }
        NativeLog.Write("airbridge_stop begin");
        NativeApi.airbridge_stop();
        NativeLog.Write("airbridge_stop end");
        _initialized = false;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            NativeApi.airbridge_shutdown();
        }
        catch (DllNotFoundException)
        {
            // Nothing to clean up if the native library was never available.
        }
        finally
        {
            _disposed = true;
            _initialized = false;
        }
    }

    private void HandleState(IntPtr userData, string? state)
    {
        try
        {
            var parsed = state switch
            {
                "connected" => PlaybackState.Connected,
                "audio" => PlaybackState.Audio,
                "mirror" => PlaybackState.Mirroring,
                "hls" => PlaybackState.Hls,
                _ => PlaybackState.Idle,
            };
            StateChanged?.Invoke(parsed);
        }
        catch
        {
            // Never let a managed exception cross the native boundary.
        }
    }

    private void HandleClient(IntPtr userData, string? name, string? model, string? deviceId)
    {
        try
        {
            ClientConnected?.Invoke(new ClientInfo
            {
                Name = name,
                Model = model,
                DeviceId = deviceId,
            });
        }
        catch
        {
        }
    }

    private void HandleMetadata(IntPtr userData, string? title, string? artist, string? album)
    {
        try
        {
            MetadataChanged?.Invoke(new NowPlaying
            {
                Title = title,
                Artist = artist,
                Album = album,
            });
        }
        catch
        {
        }
    }

    private void HandleArtwork(IntPtr userData, IntPtr data, int length)
    {
        try
        {
            if (data == IntPtr.Zero || length <= 0)
            {
                return;
            }

            var bytes = new byte[length];
            Marshal.Copy(data, bytes, 0, length);
            ArtworkChanged?.Invoke(bytes);
        }
        catch
        {
        }
    }

    private void HandleProgress(IntPtr userData, double position, double duration)
    {
        try
        {
            ProgressChanged?.Invoke(position, duration);
        }
        catch
        {
        }
    }

    private void HandleVolume(IntPtr userData, double db, double linear)
    {
        try
        {
            VolumeChanged?.Invoke(db, linear);
        }
        catch
        {
        }
    }

    private void HandlePin(IntPtr userData, string? pin)
    {
        try
        {
            if (!string.IsNullOrEmpty(pin))
            {
                PinRequired?.Invoke(pin);
            }
        }
        catch
        {
        }
    }

    private void HandleVideoSize(IntPtr userData, int width, int height)
    {
        try
        {
            VideoSizeChanged?.Invoke(width, height);
        }
        catch
        {
        }
    }

    private void HandleError(IntPtr userData, int code, string? message)
    {
        try
        {
            ErrorOccurred?.Invoke(code, message ?? "Unknown native error");
        }
        catch
        {
        }
    }

    private void HandleLog(IntPtr userData, int level, string? message)
    {
        try
        {
            if (!string.IsNullOrEmpty(message))
            {
                NativeLog.Write($"[{level}] {message}");
                LogReceived?.Invoke(level, message);
            }
        }
        catch
        {
        }
    }
}

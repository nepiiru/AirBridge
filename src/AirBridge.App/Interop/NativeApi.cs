using System.Runtime.InteropServices;

namespace AirBridge.App.Interop;

internal static class NativeApi
{
    private const string Dll = "airbridge_core";

    [StructLayout(LayoutKind.Sequential)]
    internal struct AirBridgeConfig
    {
        [MarshalAs(UnmanagedType.LPUTF8Str)] public string? ReceiverName;
        [MarshalAs(UnmanagedType.LPUTF8Str)] public string? AudioDevice;
        [MarshalAs(UnmanagedType.LPUTF8Str)] public string? VideoSink;
        public int Port;
        public int HlsEnabled;
        public int PinMode;
        [MarshalAs(UnmanagedType.LPUTF8Str)] public string? Pin;
        public int NoHold;
        public int LogLevel;
        public int AppendHostname;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void StateCallback(IntPtr userData, [MarshalAs(UnmanagedType.LPUTF8Str)] string? state);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void ClientCallback(IntPtr userData,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string? name,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string? model,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string? deviceId);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void MetadataCallback(IntPtr userData,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string? title,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string? artist,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string? album);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void ArtworkCallback(IntPtr userData, IntPtr data, int length);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void ProgressCallback(IntPtr userData, double position, double duration);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void VolumeCallback(IntPtr userData, double db, double linear);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void PinCallback(IntPtr userData, [MarshalAs(UnmanagedType.LPUTF8Str)] string? pin);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void VideoSizeCallback(IntPtr userData, int width, int height);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void ErrorCallback(IntPtr userData, int code, [MarshalAs(UnmanagedType.LPUTF8Str)] string? message);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void LogCallback(IntPtr userData, int level, [MarshalAs(UnmanagedType.LPUTF8Str)] string? message);

    [StructLayout(LayoutKind.Sequential)]
    internal struct AirBridgeCallbacks
    {
        public IntPtr UserData;
        public StateCallback? OnState;
        public ClientCallback? OnClient;
        public MetadataCallback? OnMetadata;
        public ArtworkCallback? OnArtwork;
        public ProgressCallback? OnProgress;
        public VolumeCallback? OnVolume;
        public PinCallback? OnPin;
        public VideoSizeCallback? OnVideoSize;
        public ErrorCallback? OnError;
        public LogCallback? OnLog;
    }

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr airbridge_version();

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int airbridge_init(ref AirBridgeConfig config, ref AirBridgeCallbacks callbacks);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int airbridge_start();

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void airbridge_set_window_handle(IntPtr hwnd);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void airbridge_set_volume(double db);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void airbridge_stop();

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void airbridge_shutdown();
}

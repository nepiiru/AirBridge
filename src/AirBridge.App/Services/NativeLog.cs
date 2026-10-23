using System.IO;

namespace AirBridge.App.Services;

/// <summary>
/// Small append-only diagnostic log written next to the GStreamer registry.
/// The native log callback can fire on any thread, so writes are serialized.
/// </summary>
public static class NativeLog
{
    private static readonly object Gate = new();
    private static readonly string FilePath = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "AirBridge",
        "airbridge.log");

    public static string Path => FilePath;

    public static void Write(string message)
    {
        try
        {
            lock (Gate)
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(FilePath)!);
                var info = new FileInfo(FilePath);
                if (info.Exists && info.Length > 4 * 1024 * 1024)
                {
                    File.Move(FilePath, FilePath + ".old", overwrite: true);
                }
                File.AppendAllText(FilePath,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {message}{Environment.NewLine}");
            }
        }
        catch
        {
            // Diagnostics must never take the app down.
        }
    }
}

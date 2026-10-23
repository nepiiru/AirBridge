using System.IO;

namespace AirBridge.App.Services;

/// <summary>
/// Locates the GStreamer runtime used by airbridge_core.dll.
/// Development builds use the MSYS2 UCRT64 tree; release builds can ship a
/// self-contained "gst" folder next to AirBridge.exe.
/// </summary>
public static class NativeRuntime
{
    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized)
        {
            return;
        }
        _initialized = true;

        var root = FindRoot();
        if (root == null)
        {
            return;
        }

        var bin = Path.Combine(root, "bin");
        if (Directory.Exists(bin))
        {
            var currentPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            Environment.SetEnvironmentVariable("PATH", bin + ";" + currentPath);
        }

        var plugins = Path.Combine(root, "lib", "gstreamer-1.0");
        if (Directory.Exists(plugins))
        {
            Environment.SetEnvironmentVariable("GST_PLUGIN_PATH", plugins);
            Environment.SetEnvironmentVariable("GST_PLUGIN_SYSTEM_PATH", plugins);
        }

        var scanner = Path.Combine(root, "libexec", "gstreamer-1.0", "gst-plugin-scanner.exe");
        if (File.Exists(scanner))
        {
            Environment.SetEnvironmentVariable("GST_PLUGIN_SCANNER", scanner);
        }

        var registryDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AirBridge");
        Directory.CreateDirectory(registryDirectory);
        Environment.SetEnvironmentVariable("GST_REGISTRY",
            Path.Combine(registryDirectory, "gst-registry.bin"));
    }

    private static string? FindRoot()
    {
        var bundled = Path.Combine(AppContext.BaseDirectory, "gst");
        if (Directory.Exists(Path.Combine(bundled, "bin")))
        {
            return bundled;
        }

        var configured = Environment.GetEnvironmentVariable("AIRBRIDGE_GSTREAMER_ROOT");
        if (!string.IsNullOrWhiteSpace(configured) &&
            Directory.Exists(Path.Combine(configured, "bin")))
        {
            return configured;
        }

        const string msys2 = @"P:\msys64\ucrt64";
        if (Directory.Exists(Path.Combine(msys2, "bin")))
        {
            return msys2;
        }

        return null;
    }
}

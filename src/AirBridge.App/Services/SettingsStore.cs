using System.IO;
using System.Text.Json;

namespace AirBridge.App.Services;

public sealed class SavedSettings
{
    public string ReceiverName { get; set; } = "AirBridge";
    public bool AppendHostname { get; set; } = true;
    public int Port { get; set; } = 7000;
    public string? AudioDeviceId { get; set; }
    public bool HlsEnabled { get; set; } = true;
    public int PinMode { get; set; }
    public bool NoHold { get; set; }
    public bool AutoStart { get; set; } = true;
    public double Volume { get; set; } = 50;
    public string ThemeId { get; set; } = "night";
}

public static class SettingsStore
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "AirBridge",
        "settings.json");

    public static SavedSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<SavedSettings>(json) ?? new SavedSettings();
            }
        }
        catch (Exception ex)
        {
            NativeLog.Write($"settings load failed: {ex.Message}");
        }

        return new SavedSettings();
    }

    public static void Save(SavedSettings settings)
    {
        try
        {
            var directory = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(FilePath, JsonSerializer.Serialize(settings,
                new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            NativeLog.Write($"settings save failed: {ex.Message}");
        }
    }
}

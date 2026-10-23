using System.Windows;
using AirBridge.App.Services;

namespace AirBridge.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        NativeRuntime.Initialize();
        ThemeManager.Apply(SettingsStore.Load().ThemeId);
        base.OnStartup(e);
    }
}

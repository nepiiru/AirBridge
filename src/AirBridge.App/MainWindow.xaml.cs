using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AirBridge.App.Controls;
using AirBridge.App.Services;
using AirBridge.Core.Models;
using NAudio.CoreAudioApi;

namespace AirBridge.App;

public partial class MainWindow : Window
{
    private readonly Queue<string> _logLines = new();
    private AirBridgeEngine? _engine;
    private bool _isRunning;
    private bool _startPending;
    private bool _suppressVolume;
    private bool _videoActive;
    private ClientInfo? _client;
    private string _appliedReceiverName = "AirBridge";
    private bool _appliedAppendHostname = true;
    private NowPlaying _nowPlaying = new();
    private SavedSettings _settings = new();
    private string _statusBrushKey = "MutedBrush";
    private string _videoBrushKey = "MutedBrush";
    private bool _isFullscreen;
    private bool _themeReady;
    private WindowStyle _savedWindowStyle;
    private ResizeMode _savedResizeMode;
    private WindowState _savedWindowState;
    private bool _savedTopmost;
    private Rect _savedBounds;
    private DispatcherTimer? _decorationTimer;

    public MainWindow()
    {
        InitializeComponent();

        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
        Video.HandleCreated += Video_HandleCreated;
        PinModeCombo.SelectionChanged += PinModeCombo_SelectionChanged;
        PreviewKeyDown += MainWindow_PreviewKeyDown;
        SourceInitialized += (_, _) => ThemeManager.ApplyWindowChrome(this);

        ThemeCombo.ItemsSource = ThemeCatalog.All;
        _decorationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(140) };
        _decorationTimer.Tick += (_, _) =>
        {
            _decorationTimer?.Stop();
            ThemeManager.Decorate(ThemeCanvas);
        };
        ThemeCanvas.SizeChanged += (_, _) =>
        {
            _decorationTimer?.Stop();
            _decorationTimer?.Start();
        };

        try
        {
            VersionText.Text = $"v{AirBridgeEngine.Version}";
            FooterInfo.Text = $"UxPlay {AirBridgeEngine.Version}";
        }
        catch
        {
            VersionText.Text = "v0.1.0";
            FooterInfo.Text = "native engine 未配置";
        }

        UpdateVolumeLabel(VolumeSlider.Value);
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _settings = SettingsStore.Load();
        ReceiverNameBox.Text = string.IsNullOrWhiteSpace(_settings.ReceiverName)
            ? "AirBridge"
            : _settings.ReceiverName;
        AppendHostnameCheckBox.IsChecked = _settings.AppendHostname;
        PortBox.Text = (_settings.Port is >= 1024 and <= 65535 ? _settings.Port : 7000).ToString();
        HlsCheckBox.IsChecked = _settings.HlsEnabled;
        PinModeCombo.SelectedIndex = Math.Clamp(_settings.PinMode, 0, 2);
        NoHoldCheckBox.IsChecked = _settings.NoHold;
        AutoStartCheckBox.IsChecked = _settings.AutoStart;
        VolumeSlider.Value = Math.Clamp(_settings.Volume, 0, 100);

        _themeReady = false;
        ThemeManager.Apply(_settings.ThemeId);
        ThemeCombo.SelectedValue = ThemeManager.Current.Id;
        _themeReady = true;
        ThemeManager.ApplyWindowChrome(this);

        PopulateAudioDevices(_settings.AudioDeviceId);
        UpdateNamePreview();
        ApplyState(PlaybackState.Idle);
        Dispatcher.BeginInvoke(new Action(() => ThemeManager.Decorate(ThemeCanvas)),
            DispatcherPriority.Loaded);

        if (AutoStartCheckBox.IsChecked == true)
        {
            _startPending = true;
            SetFooter("映像ウィンドウを準備しています…");
            if (Video.Handle != IntPtr.Zero)
            {
                _startPending = false;
                StartEngine();
            }
        }
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        SaveSettings();
        _engine?.Dispose();
        _engine = null;
        _isRunning = false;
    }

    private void Video_HandleCreated(object? sender, EventArgs e)
    {
        if (_startPending)
        {
            _startPending = false;
            StartEngine();
        }
        else if (_engine != null)
        {
            _engine.SetWindowHandle(Video.Handle);
        }
    }

    private void PopulateAudioDevices(string? selectedId)
    {
        AudioDeviceCombo.Items.Clear();
        AudioDeviceCombo.Items.Add(new AudioDevice { Id = "", Name = "既定のオーディオデバイス" });

        AudioDevice? selected = null;
        try
        {
            var enumerator = new MMDeviceEnumerator();
            foreach (var device in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                using (device)
                {
                    var item = new AudioDevice
                    {
                        Id = device.ID,
                        Name = device.FriendlyName,
                    };
                    AudioDeviceCombo.Items.Add(item);
                    if (!string.IsNullOrEmpty(selectedId) && item.Id == selectedId)
                    {
                        selected = item;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            AppendLog($"オーディオデバイスの列挙に失敗しました: {ex.Message}");
        }

        AudioDeviceCombo.SelectedItem = selected ?? AudioDeviceCombo.Items[0];
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isRunning)
        {
            StopEngine();
        }
        else
        {
            StartEngine();
        }
    }

    private void StartEngine()
    {
        if (_isRunning)
        {
            return;
        }

        if (Video.Handle == IntPtr.Zero)
        {
            _startPending = true;
            SetFooter("映像ウィンドウを準備しています…");
            return;
        }

        var name = ReceiverNameBox.Text.Trim();
        if (name.Length == 0)
        {
            ShowStartError("デバイス名を入力してください。");
            return;
        }

        if (!int.TryParse(PortBox.Text.Trim(), out var port) || port < 1024 || port > 65535)
        {
            ShowStartError("ポートは 1024〜65535 の範囲で指定してください。");
            return;
        }

        var pinMode = (PinMode)Math.Clamp(PinModeCombo.SelectedIndex, 0, 2);
        if (pinMode == PinMode.Password && PasswordInput.Password.Length < 4)
        {
            ShowStartError("パスワードは4文字以上で入力してください。");
            return;
        }

        var settings = new ReceiverSettings
        {
            ReceiverName = name,
            AppendHostname = AppendHostnameCheckBox.IsChecked == true,
            Port = port,
            AudioDeviceId = (AudioDeviceCombo.SelectedItem as AudioDevice)?.Id is { Length: > 0 } id
                ? id
                : null,
            HlsEnabled = HlsCheckBox.IsChecked == true,
            PinMode = pinMode,
            Password = PasswordInput.Password,
            NoHold = NoHoldCheckBox.IsChecked == true,
            VolumeDb = SliderToDb(VolumeSlider.Value),
        };

        var engine = new AirBridgeEngine();
        engine.StateChanged += state => Dispatch(() => ApplyState(state));
        engine.ClientConnected += client => Dispatch(() => ApplyClient(client));
        engine.MetadataChanged += metadata => Dispatch(() => ApplyMetadata(metadata));
        engine.ArtworkChanged += artwork => Dispatch(() => ApplyArtwork(artwork));
        engine.ProgressChanged += (position, duration) => Dispatch(() => ApplyProgress(position, duration));
        engine.VolumeChanged += (db, _) => Dispatch(() => ApplyRemoteVolume(db));
        engine.PinRequired += pin => Dispatch(() => ApplyPin(pin));
        engine.VideoSizeChanged += (width, height) => Dispatch(() => ApplyVideoSize(width, height));
        engine.ErrorOccurred += (code, message) => Dispatch(() => ShowRuntimeError(code, message));
        engine.LogReceived += (level, message) => Dispatch(() => AppendNativeLog(level, message));

        try
        {
            engine.Initialize(settings);
            engine.SetWindowHandle(Video.Handle);
            engine.SetVolume(settings.VolumeDb);
            engine.Start();
        }
        catch (DllNotFoundException)
        {
            engine.Dispose();
            ShowStartError("airbridge_core.dll が見つかりません。native エンジンをビルドして配置してください。");
            return;
        }
        catch (BadImageFormatException)
        {
            engine.Dispose();
            ShowStartError("airbridge_core.dll のアーキテクチャが一致しません（x64 が必要です）。");
            return;
        }
        catch (Exception ex)
        {
            engine.Dispose();
            ShowStartError($"エンジンの起動に失敗しました: {ex.Message}");
            return;
        }

        _engine = engine;
        _isRunning = true;
        _appliedReceiverName = name;
        _appliedAppendHostname = settings.AppendHostname;
        _videoActive = false;
        _client = null;
        _nowPlaying = new NowPlaying();
        ClearPin();
        SaveSettings();
        ApplyRunningUi(true);
        ApplyState(PlaybackState.Idle);
        var displayName = BuildDisplayName(name, settings.AppendHostname);
        SetFooter($"「{displayName}」で待機中（port {port}）");
        AppendLog($"エンジンを開始しました（{displayName}, port {port}）");
    }

    private void StopEngine()
    {
        var engine = _engine;
        _engine = null;
        _isRunning = false;

        if (engine != null)
        {
            try
            {
                engine.Stop();
            }
            catch (Exception ex)
            {
                AppendLog($"停止処理でエラー: {ex.Message}");
            }
            finally
            {
                engine.Dispose();
            }
        }

        _videoActive = false;
        _client = null;
        ClearPin();
        SaveSettings();
        ApplyRunningUi(false);
        ApplyState(PlaybackState.Idle);
        AppendLog("エンジンを停止しました");
    }

    private void ApplyRunningUi(bool running)
    {
        StartButton.Content = running ? "停止" : "開始";
        StartButton.Style = (Style)FindResource(running ? "DangerButton" : "PrimaryButton");

        ReceiverNameBox.IsEnabled = true;
        AppendHostnameCheckBox.IsEnabled = true;
        PortBox.IsEnabled = !running;
        HlsCheckBox.IsEnabled = !running;
        PinModeCombo.IsEnabled = !running;
        PasswordInput.IsEnabled = !running;
        NoHoldCheckBox.IsEnabled = !running;
        AudioDeviceCombo.IsEnabled = !running;
        AutoStartCheckBox.IsEnabled = !running;

        UpdateNamePreview();
    }

    private void ReceiverNameBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        UpdateNamePreview();
    }

    private void ReceiverNameOption_Changed(object sender, RoutedEventArgs e)
    {
        UpdateNamePreview();
    }

    private void ApplyNameButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_isRunning || !IsNameDirty())
        {
            return;
        }

        var name = ReceiverNameBox.Text.Trim();
        if (name.Length == 0)
        {
            MessageBox.Show(this, "デバイス名を入力してください。", "AirBridge",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_client != null || PinPanel.Visibility == Visibility.Visible)
        {
            var result = MessageBox.Show(this,
                "名前を変更すると、接続中の AirPlay セッションが切断されます。\nレシーバーを再起動して適用しますか？",
                "AirBridge", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }
        }

        StopEngine();
        StartEngine();
    }

    private bool IsNameDirty()
    {
        return _isRunning &&
               (ReceiverNameBox.Text.Trim() != _appliedReceiverName ||
                (AppendHostnameCheckBox.IsChecked == true) != _appliedAppendHostname);
    }

    private void UpdateNamePreview()
    {
        if (ReceiverNameBox == null || AppendHostnameCheckBox == null ||
            ReceiverNamePreview == null || ApplyNameButton == null || StartHint == null)
        {
            return;
        }

        var name = ReceiverNameBox.Text.Trim();
        var appendHostname = AppendHostnameCheckBox.IsChecked == true;
        ReceiverNamePreview.Text = $"AirPlay メニュー表示名: {BuildDisplayName(name, appendHostname)}";

        var dirty = IsNameDirty();
        ApplyNameButton.Visibility = dirty ? Visibility.Visible : Visibility.Collapsed;

        if (_isRunning)
        {
            StartHint.Text = dirty
                ? "変更を反映するには「再起動して適用」を押してください。接続中の場合は切断されます。"
                : $"AirPlay メニューに「{BuildDisplayName(_appliedReceiverName, _appliedAppendHostname)}」が表示されています。";
        }
        else
        {
            StartHint.Text = "開始すると、同じネットワーク上の Apple デバイスから選択できます。";
        }
    }

    private static string BuildDisplayName(string name, bool appendHostname)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "（デバイス名を入力）";
        }

        return appendHostname ? $"{name}@{Environment.MachineName}" : name;
    }

    private void ThemeCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (!_themeReady || ThemeCombo.SelectedValue is not string themeId)
        {
            return;
        }

        ThemeManager.Apply(themeId);
        ThemeManager.ApplyWindowChrome(this);
        ThemeManager.Decorate(ThemeCanvas);
        StatusDot.Fill = (Brush)FindResource(_statusBrushKey);
        VideoDot.Fill = (Brush)FindResource(_videoBrushKey);
        SaveSettings();
    }

    private void FullscreenButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleFullscreen();
    }

    private void ExitFullscreenButton_Click(object sender, RoutedEventArgs e)
    {
        ExitFullscreen();
    }

    private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F11)
        {
            ToggleFullscreen();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && _isFullscreen)
        {
            ExitFullscreen();
            e.Handled = true;
        }
    }

    private void ToggleFullscreen()
    {
        if (_isFullscreen)
        {
            ExitFullscreen();
        }
        else
        {
            EnterFullscreen();
        }
    }

    private void EnterFullscreen()
    {
        if (_isFullscreen)
        {
            return;
        }

        _isFullscreen = true;
        _savedWindowStyle = WindowStyle;
        _savedResizeMode = ResizeMode;
        _savedWindowState = WindowState;
        _savedTopmost = Topmost;
        _savedBounds = WindowState == WindowState.Normal
            ? new Rect(Left, Top, ActualWidth, ActualHeight)
            : RestoreBounds;

        WindowState = WindowState.Normal;
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        Left = SystemParameters.VirtualScreenLeft;
        Top = SystemParameters.VirtualScreenTop;
        Width = SystemParameters.VirtualScreenWidth;
        Height = SystemParameters.VirtualScreenHeight;
        Topmost = true;

        ApplyFullscreenChrome(true);
        Focus();
        UpdateDecorationsSoon();
    }

    private void ExitFullscreen()
    {
        if (!_isFullscreen)
        {
            return;
        }

        _isFullscreen = false;
        WindowStyle = _savedWindowStyle;
        ResizeMode = _savedResizeMode;
        Topmost = _savedTopmost;

        if (_savedWindowState == WindowState.Maximized)
        {
            WindowState = WindowState.Normal;
            WindowState = WindowState.Maximized;
        }
        else
        {
            WindowState = WindowState.Normal;
            Left = _savedBounds.Left;
            Top = _savedBounds.Top;
            Width = _savedBounds.Width;
            Height = _savedBounds.Height;
        }

        ApplyFullscreenChrome(false);
        ThemeManager.ApplyWindowChrome(this);
        UpdateDecorationsSoon();
    }

    private void ApplyFullscreenChrome(bool fullscreen)
    {
        HeaderArea.Visibility = fullscreen ? Visibility.Collapsed : Visibility.Visible;
        FullscreenBar.Visibility = fullscreen ? Visibility.Visible : Visibility.Collapsed;
        FooterArea.Visibility = fullscreen ? Visibility.Collapsed : Visibility.Visible;
        SidePanel.Visibility = fullscreen ? Visibility.Collapsed : Visibility.Visible;
        VideoStatusBar.Visibility = fullscreen ? Visibility.Collapsed : Visibility.Visible;

        HeaderSpacerRow.Height = new GridLength(fullscreen ? 0 : 20);
        FooterSpacerRow.Height = new GridLength(fullscreen ? 0 : 14);
        SideSpacerColumn.Width = new GridLength(fullscreen ? 0 : 20);
        SideColumn.Width = new GridLength(fullscreen ? 0 : 390);

        ChromeGrid.Margin = new Thickness(fullscreen ? 0 : 24);
        VideoBorder.CornerRadius = new CornerRadius(fullscreen ? 0 : 14);
        VideoBorder.BorderThickness = new Thickness(fullscreen ? 0 : 1);
        FullscreenButton.Content = fullscreen ? "全画面を終了" : "全画面";
    }

    private void UpdateDecorationsSoon()
    {
        Dispatcher.BeginInvoke(new Action(() => ThemeManager.Decorate(ThemeCanvas)),
            DispatcherPriority.Loaded);
    }

    private void ApplyState(PlaybackState state)
    {
        if (_isRunning == false && state != PlaybackState.Idle)
        {
            return;
        }

        switch (state)
        {
            case PlaybackState.Idle:
                _videoActive = false;
                ClearPin();
                ClearNowPlayingUi();
                SetStatus("待機中", "MutedBrush");
                SetFooter("待機中");
                UpdateVideoStatus("待機中", "AirPlay 接続を待っています", "MutedBrush");
                VideoResolutionText.Text = "";
                ModeText.Text = "モード: —";
                break;

            case PlaybackState.Connected:
                _videoActive = false;
                SetStatus("接続済み", "SuccessBrush");
                SetFooter(_client?.Name is { Length: > 0 } clientName
                    ? $"{clientName} が接続しました"
                    : "Apple デバイスが接続しました");
                UpdateVideoStatus("接続済み", "オーディオまたは画面を送信してください", "SuccessBrush");
                VideoResolutionText.Text = "";
                ModeText.Text = "モード: 接続済み";
                break;

            case PlaybackState.Audio:
                if (_videoActive)
                {
                    // Audio metadata can arrive during a video session; keep the video visible.
                    return;
                }
                SetStatus("オーディオ", "AccentBrush");
                SetFooter("オーディオを再生中");
                UpdateVideoStatus("オーディオ", "映像なしで再生中", "AccentBrush");
                VideoResolutionText.Text = "";
                ModeText.Text = "モード: オーディオ";
                break;

            case PlaybackState.Mirroring:
                _videoActive = true;
                SetStatus("ミラーリング", "AccentBrush");
                SetFooter("画面をミラーリング中");
                UpdateVideoStatus("ミラーリング", "画面を表示しています", "AccentBrush");
                ModeText.Text = "モード: 画面ミラーリング";
                break;

            case PlaybackState.Hls:
                _videoActive = true;
                // HLS (YouTube app) does not provide DMAP metadata, so clear
                // any audio track left over from the previous session.
                ClearNowPlayingUi();
                SetStatus("ビデオ", "AccentBrush");
                SetFooter("HLS ビデオを再生中");
                UpdateVideoStatus("ビデオ", "HLS ビデオを再生中", "AccentBrush");
                ModeText.Text = "モード: HLS ビデオ";
                break;
        }
    }

    private void UpdateVideoStatus(string title, string subtitle, string brushKey)
    {
        VideoStatusText.Text = title;
        VideoSubStatusText.Text = subtitle;
        _videoBrushKey = brushKey;
        VideoDot.Fill = (Brush)FindResource(brushKey);
    }

    private void ApplyClient(ClientInfo client)
    {
        _client = client;
        var name = string.IsNullOrWhiteSpace(client.Name) ? "不明なデバイス" : client.Name;
        var model = string.IsNullOrWhiteSpace(client.Model) ? "" : $" ({client.Model})";
        ClientText.Text = $"クライアント: {name}{model}";
        SetFooter($"{name} が接続しました");
        if (PinPanel.Visibility != Visibility.Visible)
        {
            SetStatus("接続済み", "SuccessBrush");
        }
    }

    private void ClearNowPlayingUi()
    {
        _nowPlaying = new NowPlaying();
        TrackTitle.Text = "—";
        TrackArtist.Text = "";
        TrackAlbum.Text = "";
        ArtworkImage.Source = null;
        ArtworkImage.Visibility = Visibility.Collapsed;
        ArtworkFallback.Visibility = Visibility.Visible;
        Progress.Value = 0;
        PositionText.Text = "0:00";
        DurationText.Text = "0:00";
    }

    private void ApplyMetadata(NowPlaying metadata)
    {
        var titleChanged = !string.Equals(_nowPlaying.Title, metadata.Title, StringComparison.Ordinal);
        _nowPlaying.Title = metadata.Title;
        _nowPlaying.Artist = metadata.Artist;
        _nowPlaying.Album = metadata.Album;

        TrackTitle.Text = string.IsNullOrWhiteSpace(metadata.Title) ? "—" : metadata.Title;
        TrackArtist.Text = metadata.Artist ?? "";
        TrackAlbum.Text = metadata.Album ?? "";

        if (titleChanged || string.IsNullOrWhiteSpace(metadata.Title))
        {
            ArtworkImage.Source = null;
            ArtworkImage.Visibility = Visibility.Collapsed;
            ArtworkFallback.Visibility = Visibility.Visible;
        }
    }

    private void ApplyArtwork(byte[] bytes)
    {
        try
        {
            using var stream = new MemoryStream(bytes);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            image.StreamSource = stream;
            image.EndInit();
            image.Freeze();

            ArtworkImage.Source = image;
            ArtworkImage.Visibility = Visibility.Visible;
            ArtworkFallback.Visibility = Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            AppendLog($"カバーアートの読み込みに失敗しました: {ex.Message}");
        }
    }

    private void ApplyProgress(double position, double duration)
    {
        if (duration <= 0 || double.IsNaN(duration) || double.IsInfinity(duration))
        {
            Progress.Value = 0;
            PositionText.Text = "0:00";
            DurationText.Text = "0:00";
            return;
        }

        position = Math.Clamp(position, 0, duration);
        Progress.Value = duration > 0 ? position / duration * 100.0 : 0;
        PositionText.Text = FormatTime(position);
        DurationText.Text = FormatTime(duration);
    }

    private void ApplyRemoteVolume(double db)
    {
        var percent = db <= -100 ? 0 : Math.Clamp((db + 30.0) / 30.0 * 100.0, 0, 100);
        _suppressVolume = true;
        VolumeSlider.Value = Math.Round(percent);
        _suppressVolume = false;
        UpdateVolumeLabel(VolumeSlider.Value);
    }

    private void ApplyPin(string pin)
    {
        PinText.Text = pin;
        PinPanel.Visibility = Visibility.Visible;
        SetStatus("PIN を入力", "AccentBrush");
        SetFooter($"接続先の Apple デバイスに PIN {pin} を入力してください");
    }

    private void ClearPin()
    {
        PinPanel.Visibility = Visibility.Collapsed;
        PinText.Text = "0000";
    }

    private void ApplyVideoSize(int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            return;
        }
        ModeText.Text = $"モード: ミラーリング {width}×{height}";
        VideoResolutionText.Text = $"{width} × {height}";
        SetFooter($"画面をミラーリング中（{width}×{height}）");
    }

    private void ShowRuntimeError(int code, string message)
    {
        AppendLog($"エラー {code}: {message}");
        SetStatus("エラー", "DangerBrush");
        SetFooter(message);
    }

    private void ShowStartError(string message)
    {
        _startPending = false;
        ApplyRunningUi(false);
        SetStatus("起動エラー", "DangerBrush");
        SetFooter(message);
        AppendLog(message);
        MessageBox.Show(this, message, "AirBridge", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void PinModeCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        var isPassword = PinModeCombo.SelectedIndex == 2;
        if (PasswordInput == null || PasswordHint == null)
        {
            return;
        }
        PasswordInput.Visibility = isPassword ? Visibility.Visible : Visibility.Collapsed;
        PasswordHint.Visibility = isPassword ? Visibility.Visible : Visibility.Collapsed;
    }

    private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (VolumeText == null)
        {
            return;
        }

        UpdateVolumeLabel(e.NewValue);
        if (_suppressVolume || !_isRunning || _engine == null)
        {
            return;
        }

        _engine.SetVolume(SliderToDb(e.NewValue));
    }

    private void UpdateVolumeLabel(double value)
    {
        if (VolumeText != null)
        {
            VolumeText.Text = $"{Math.Round(value):0}%";
        }
    }

    private static double SliderToDb(double value)
    {
        if (value <= 0.5)
        {
            return -144.0;
        }
        return -30.0 + 30.0 * (value / 100.0);
    }

    private void SaveSettings()
    {
        if (ReceiverNameBox == null || AppendHostnameCheckBox == null ||
            PortBox == null || VolumeSlider == null)
        {
            return;
        }

        var name = ReceiverNameBox.Text.Trim();
        if (name.Length == 0)
        {
            name = "AirBridge";
        }

        if (!int.TryParse(PortBox.Text.Trim(), out var port) || port < 1024 || port > 65535)
        {
            port = 7000;
        }

        _settings = new SavedSettings
        {
            ReceiverName = name,
            AppendHostname = AppendHostnameCheckBox.IsChecked == true,
            Port = port,
            AudioDeviceId = (AudioDeviceCombo.SelectedItem as AudioDevice)?.Id,
            HlsEnabled = HlsCheckBox.IsChecked == true,
            PinMode = Math.Clamp(PinModeCombo.SelectedIndex, 0, 2),
            NoHold = NoHoldCheckBox.IsChecked == true,
            AutoStart = AutoStartCheckBox.IsChecked == true,
            Volume = VolumeSlider.Value,
            ThemeId = ThemeManager.Current.Id,
        };
        SettingsStore.Save(_settings);
    }

    private void ClearLogButton_Click(object sender, RoutedEventArgs e)
    {
        _logLines.Clear();
        LogBox.Clear();
    }

    private void AppendNativeLog(int level, string message)
    {
        var prefix = level switch
        {
            0 => "ERR ",
            1 => "WARN",
            2 => "INFO",
            _ => "DBG ",
        };
        AppendLog($"{prefix} {message}");
    }

    private void AppendLog(string message)
    {
        if (Dispatcher.HasShutdownStarted)
        {
            return;
        }

        _logLines.Enqueue($"[{DateTime.Now:HH:mm:ss}] {message}");
        while (_logLines.Count > 300)
        {
            _logLines.Dequeue();
        }

        LogBox.Text = string.Join(Environment.NewLine, _logLines);
        LogBox.ScrollToEnd();
    }

    private void SetStatus(string text, string brushKey)
    {
        StatusText.Text = text;
        _statusBrushKey = brushKey;
        StatusDot.Fill = (Brush)FindResource(brushKey);
    }

    private void SetFooter(string text)
    {
        FooterStatus.Text = text;
    }

    private static string FormatTime(double seconds)
    {
        if (seconds < 0 || double.IsNaN(seconds) || double.IsInfinity(seconds))
        {
            return "0:00";
        }

        var time = TimeSpan.FromSeconds(seconds);
        return time.TotalHours >= 1
            ? $"{(int)time.TotalHours}:{time.Minutes:00}:{time.Seconds:00}"
            : $"{time.Minutes}:{time.Seconds:00}";
    }

    private void Dispatch(Action action)
    {
        if (Dispatcher.HasShutdownStarted || Dispatcher.HasShutdownFinished)
        {
            return;
        }

        if (Dispatcher.CheckAccess())
        {
            action();
        }
        else
        {
            Dispatcher.BeginInvoke(action);
        }
    }
}

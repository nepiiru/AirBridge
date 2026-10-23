using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace AirBridge.App.Services;

public enum ThemeDecoration
{
    None,
    Bokeh,
    Clouds,
    Stars,
    Sunset,
    Leaves,
    Grid,
    Petals,
}

public sealed class ThemeDefinition
{
    public string Id { get; init; } = "";
    public string DisplayName { get; init; } = "";
    public bool IsLight { get; init; }
    public Color BackgroundTop { get; init; }
    public Color BackgroundBottom { get; init; }
    public Color Surface { get; init; }
    public Color Surface2 { get; init; }
    public Color Surface3 { get; init; }
    public Color Text { get; init; }
    public Color Muted { get; init; }
    public Color Accent { get; init; }
    public Color AccentDark { get; init; }
    public Color Success { get; init; }
    public Color Danger { get; init; }
    public Color Border { get; init; }
    public Color OnAccent { get; init; }
    public Color OnDanger { get; init; }
    public Color VideoBackground { get; init; }
    public Color LogBackground { get; init; }
    public Color DecorationPrimary { get; init; }
    public Color DecorationSecondary { get; init; }
    public ThemeDecoration Decoration { get; init; }

    public override string ToString() => DisplayName;
}

public static class ThemeCatalog
{
    public static IReadOnlyList<ThemeDefinition> All { get; } =
    [
        new ThemeDefinition
        {
            Id = "modern",
            DisplayName = "モダン",
            IsLight = true,
            BackgroundTop = Hex("#F8F1E7"),
            BackgroundBottom = Hex("#E3D0BB"),
            Surface = Hex("#F7FFFFFF"),
            Surface2 = Hex("#FFF8F0"),
            Surface3 = Hex("#EAD8C6"),
            Text = Hex("#3B2A1E"),
            Muted = Hex("#8A6F5C"),
            Accent = Hex("#A9663B"),
            AccentDark = Hex("#7E4527"),
            Success = Hex("#3F8F5F"),
            Danger = Hex("#C94B4B"),
            Border = Hex("#D8C2AD"),
            OnAccent = Hex("#FFFFFF"),
            OnDanger = Hex("#FFFFFF"),
            VideoBackground = Hex("#1B120C"),
            LogBackground = Hex("#F3E8DC"),
            DecorationPrimary = Hex("#B97A4F"),
            DecorationSecondary = Hex("#FFFFFF"),
            Decoration = ThemeDecoration.Bokeh,
        },
        new ThemeDefinition
        {
            Id = "sky",
            DisplayName = "空",
            IsLight = true,
            BackgroundTop = Hex("#E9F9FF"),
            BackgroundBottom = Hex("#A9DEFF"),
            Surface = Hex("#F2FFFFFF"),
            Surface2 = Hex("#FFF9FDFF"),
            Surface3 = Hex("#D7EEFA"),
            Text = Hex("#173449"),
            Muted = Hex("#557F96"),
            Accent = Hex("#1B9BD8"),
            AccentDark = Hex("#0F6D9B"),
            Success = Hex("#2E9E6B"),
            Danger = Hex("#D45B5B"),
            Border = Hex("#BFE0EF"),
            OnAccent = Hex("#FFFFFF"),
            OnDanger = Hex("#FFFFFF"),
            VideoBackground = Hex("#06121A"),
            LogBackground = Hex("#F0FAFF"),
            DecorationPrimary = Hex("#FFFFFF"),
            DecorationSecondary = Hex("#D8F2FF"),
            Decoration = ThemeDecoration.Clouds,
        },
        new ThemeDefinition
        {
            Id = "night",
            DisplayName = "夜空",
            IsLight = false,
            BackgroundTop = Hex("#050814"),
            BackgroundBottom = Hex("#142443"),
            Surface = Hex("#E0101A2B"),
            Surface2 = Hex("#E0182438"),
            Surface3 = Hex("#E0233149"),
            Text = Hex("#F2F6FF"),
            Muted = Hex("#9EAECB"),
            Accent = Hex("#7FCBFF"),
            AccentDark = Hex("#1D5B80"),
            Success = Hex("#5FE0A0"),
            Danger = Hex("#FF7A7A"),
            Border = Hex("#33456A"),
            OnAccent = Hex("#06131F"),
            OnDanger = Hex("#240A0A"),
            VideoBackground = Hex("#03050A"),
            LogBackground = Hex("#B0080D16"),
            DecorationPrimary = Hex("#FFFFFF"),
            DecorationSecondary = Hex("#9DD8FF"),
            Decoration = ThemeDecoration.Stars,
        },
        new ThemeDefinition
        {
            Id = "sunset",
            DisplayName = "夕焼け",
            IsLight = false,
            BackgroundTop = Hex("#20102D"),
            BackgroundBottom = Hex("#B74B3C"),
            Surface = Hex("#E0221628"),
            Surface2 = Hex("#E02C1C32"),
            Surface3 = Hex("#E03A2740"),
            Text = Hex("#FFF3EA"),
            Muted = Hex("#D3A99B"),
            Accent = Hex("#FF9E5E"),
            AccentDark = Hex("#A64E2D"),
            Success = Hex("#F2C14E"),
            Danger = Hex("#FF6B6B"),
            Border = Hex("#5A3548"),
            OnAccent = Hex("#2A1208"),
            OnDanger = Hex("#240A0A"),
            VideoBackground = Hex("#0B0508"),
            LogBackground = Hex("#C0150B16"),
            DecorationPrimary = Hex("#FFD08A"),
            DecorationSecondary = Hex("#FF7E5F"),
            Decoration = ThemeDecoration.Sunset,
        },
        new ThemeDefinition
        {
            Id = "forest",
            DisplayName = "フォレスト",
            IsLight = false,
            BackgroundTop = Hex("#07140E"),
            BackgroundBottom = Hex("#193A29"),
            Surface = Hex("#E00F2018"),
            Surface2 = Hex("#E0162B20"),
            Surface3 = Hex("#E0203B2C"),
            Text = Hex("#EAF6EE"),
            Muted = Hex("#9BB8A8"),
            Accent = Hex("#6FD39A"),
            AccentDark = Hex("#2E6B4D"),
            Success = Hex("#7BE0A8"),
            Danger = Hex("#FF7B7B"),
            Border = Hex("#2F4E3C"),
            OnAccent = Hex("#061A10"),
            OnDanger = Hex("#240A0A"),
            VideoBackground = Hex("#030906"),
            LogBackground = Hex("#B007120C"),
            DecorationPrimary = Hex("#6FD39A"),
            DecorationSecondary = Hex("#2E6B4D"),
            Decoration = ThemeDecoration.Leaves,
        },
        new ThemeDefinition
        {
            Id = "mono",
            DisplayName = "モノクロ",
            IsLight = false,
            BackgroundTop = Hex("#0E0F12"),
            BackgroundBottom = Hex("#22252B"),
            Surface = Hex("#E01A1C21"),
            Surface2 = Hex("#E023262D"),
            Surface3 = Hex("#E02E3139"),
            Text = Hex("#F2F3F5"),
            Muted = Hex("#A2A6AE"),
            Accent = Hex("#D8DADF"),
            AccentDark = Hex("#555961"),
            Success = Hex("#9BD3B0"),
            Danger = Hex("#FF8080"),
            Border = Hex("#393C43"),
            OnAccent = Hex("#101114"),
            OnDanger = Hex("#240A0A"),
            VideoBackground = Hex("#050607"),
            LogBackground = Hex("#B00A0B0D"),
            DecorationPrimary = Hex("#D8DADF"),
            DecorationSecondary = Hex("#7A7E87"),
            Decoration = ThemeDecoration.Grid,
        },
        new ThemeDefinition
        {
            Id = "sakura",
            DisplayName = "桜",
            IsLight = true,
            BackgroundTop = Hex("#FFF5F8"),
            BackgroundBottom = Hex("#FFD9E7"),
            Surface = Hex("#F7FFFFFF"),
            Surface2 = Hex("#FFF0F5"),
            Surface3 = Hex("#F8DCE7"),
            Text = Hex("#4A2433"),
            Muted = Hex("#966C7C"),
            Accent = Hex("#E06A99"),
            AccentDark = Hex("#A63D68"),
            Success = Hex("#4CAF82"),
            Danger = Hex("#D45555"),
            Border = Hex("#F0C5D4"),
            OnAccent = Hex("#FFFFFF"),
            OnDanger = Hex("#FFFFFF"),
            VideoBackground = Hex("#1A0A10"),
            LogBackground = Hex("#FFF7FA"),
            DecorationPrimary = Hex("#F2A7C2"),
            DecorationSecondary = Hex("#FFFFFF"),
            Decoration = ThemeDecoration.Petals,
        },
    ];

    public static ThemeDefinition Default => All.First(theme => theme.Id == "night");

    public static ThemeDefinition Get(string? id)
    {
        return All.FirstOrDefault(theme =>
                   string.Equals(theme.Id, id, StringComparison.OrdinalIgnoreCase))
               ?? Default;
    }

    private static Color Hex(string value) => (Color)ColorConverter.ConvertFromString(value)!;
}

public static class ThemeManager
{
    private const int DwmUseImmersiveDarkMode = 20;

    public static ThemeDefinition Current { get; private set; } = ThemeCatalog.Default;

    public static event Action? ThemeChanged;

    public static void Apply(string? id)
    {
        Current = ThemeCatalog.Get(id);
        var resources = Application.Current?.Resources;
        if (resources == null)
        {
            return;
        }

        resources["BgBrush"] = new LinearGradientBrush(
            Current.BackgroundTop,
            Current.BackgroundBottom,
            new Point(0, 0),
            new Point(1, 1));

        resources["SurfaceBrush"] = new SolidColorBrush(Current.Surface);
        resources["Surface2Brush"] = new SolidColorBrush(Current.Surface2);
        resources["Surface3Brush"] = new SolidColorBrush(Current.Surface3);
        resources["TextBrush"] = new SolidColorBrush(Current.Text);
        resources["MutedBrush"] = new SolidColorBrush(Current.Muted);
        resources["AccentBrush"] = new SolidColorBrush(Current.Accent);
        resources["AccentDarkBrush"] = new SolidColorBrush(Current.AccentDark);
        resources["SuccessBrush"] = new SolidColorBrush(Current.Success);
        resources["DangerBrush"] = new SolidColorBrush(Current.Danger);
        resources["BorderBrush"] = new SolidColorBrush(Current.Border);
        resources["AccentForegroundBrush"] = new SolidColorBrush(Current.OnAccent);
        resources["PrimaryButtonForegroundBrush"] = new SolidColorBrush(Current.OnAccent);
        resources["DangerForegroundBrush"] = new SolidColorBrush(Current.OnDanger);
        resources["VideoBackgroundBrush"] = new SolidColorBrush(Current.VideoBackground);
        resources["LogBackgroundBrush"] = new SolidColorBrush(Current.LogBackground);
        resources["ArtworkFallbackBrush"] = new SolidColorBrush(Current.Surface3);

        ThemeChanged?.Invoke();
    }

    public static void ApplyWindowChrome(Window window)
    {
        try
        {
            var handle = new WindowInteropHelper(window).Handle;
            if (handle == IntPtr.Zero)
            {
                return;
            }

            var dark = Current.IsLight ? 0 : 1;
            DwmSetWindowAttribute(handle, DwmUseImmersiveDarkMode, ref dark, sizeof(int));
        }
        catch
        {
            // Older Windows versions do not support the immersive dark title bar.
        }
    }

    public static void Decorate(Canvas canvas)
    {
        canvas.Children.Clear();
        var width = canvas.ActualWidth;
        var height = canvas.ActualHeight;
        if (width <= 1 || height <= 1)
        {
            return;
        }

        switch (Current.Decoration)
        {
            case ThemeDecoration.Stars:
                AddStars(canvas, width, height);
                break;
            case ThemeDecoration.Clouds:
                AddClouds(canvas, width, height);
                break;
            case ThemeDecoration.Bokeh:
                AddBokeh(canvas, width, height);
                break;
            case ThemeDecoration.Sunset:
                AddSunset(canvas, width, height);
                break;
            case ThemeDecoration.Leaves:
                AddLeaves(canvas, width, height);
                break;
            case ThemeDecoration.Grid:
                AddGrid(canvas, width, height);
                break;
            case ThemeDecoration.Petals:
                AddPetals(canvas, width, height);
                break;
        }
    }

    private static void AddStars(Canvas canvas, double width, double height)
    {
        var random = new Random(20261023);
        var moonColor = Color.FromRgb(255, 244, 203);
        var moonX = width * 0.58;
        var moonY = height * 0.085;

        AddGlow(canvas, moonX, moonY, 190, moonColor, 0.17);
        var moon = new Ellipse
        {
            Width = 38,
            Height = 38,
            Fill = new SolidColorBrush(moonColor),
            Opacity = 0.92,
            IsHitTestVisible = false,
            Effect = new DropShadowEffect
            {
                Color = moonColor,
                BlurRadius = 28,
                ShadowDepth = 0,
                Opacity = 0.9,
            },
        };
        Canvas.SetLeft(moon, moonX - 19);
        Canvas.SetTop(moon, moonY - 19);
        canvas.Children.Add(moon);

        var count = Math.Clamp((int)(width * height / 7000), 85, 210);
        for (var i = 0; i < count; i++)
        {
            var size = 1.1 + random.NextDouble() * 3.0;
            var opacity = 0.3 + random.NextDouble() * 0.7;
            var star = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = Brush(Current.DecorationPrimary),
                IsHitTestVisible = false,
            };

            if (size > 2.7)
            {
                star.Effect = new DropShadowEffect
                {
                    Color = Current.DecorationPrimary,
                    BlurRadius = 9,
                    ShadowDepth = 0,
                    Opacity = 0.9,
                };
            }

            Canvas.SetLeft(star, random.NextDouble() * width);
            Canvas.SetTop(star, random.NextDouble() * height);
            canvas.Children.Add(star);
            Twinkle(star, opacity, random);
        }
    }

    private static void AddClouds(Canvas canvas, double width, double height)
    {
        AddCloud(canvas, width * 0.03, height * 0.17, 0.95, 0.54, 0);
        AddCloud(canvas, width * 0.56, height * 0.08, 1.25, 0.46, 4);
        AddCloud(canvas, width * 0.32, height * 0.54, 1.45, 0.34, 2);
        AddCloud(canvas, width * 0.70, height * 0.67, 0.85, 0.42, 7);
        AddCloud(canvas, width * 0.06, height * 0.77, 0.70, 0.28, 10);
    }

    private static void AddCloud(Canvas canvas, double left, double top, double scale, double opacity, double delay)
    {
        var cloud = new Canvas
        {
            Width = 230 * scale,
            Height = 100 * scale,
            Opacity = opacity,
            IsHitTestVisible = false,
            Effect = new BlurEffect { Radius = 7 * scale },
        };

        var fill = new SolidColorBrush(Colors.White);
        AddEllipse(cloud, 8, 44, 116, 48, fill);
        AddEllipse(cloud, 44, 18, 108, 68, fill);
        AddEllipse(cloud, 105, 34, 112, 54, fill);
        AddEllipse(cloud, 64, 52, 130, 40, fill);

        var transform = new TranslateTransform();
        cloud.RenderTransform = transform;
        transform.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation
        {
            From = -12 * scale,
            To = 30 * scale,
            Duration = TimeSpan.FromSeconds(34 + delay * 4),
            BeginTime = TimeSpan.FromSeconds(delay),
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut },
        });

        Canvas.SetLeft(cloud, left);
        Canvas.SetTop(cloud, top);
        canvas.Children.Add(cloud);
    }

    private static void AddBokeh(Canvas canvas, double width, double height)
    {
        var random = new Random(7719);
        for (var i = 0; i < 12; i++)
        {
            var size = 46 + random.NextDouble() * 190;
            var color = i % 3 == 0 ? Current.DecorationSecondary : Current.DecorationPrimary;
            var circle = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = GlowBrush(color, 0.30),
                Opacity = 0.25 + random.NextDouble() * 0.3,
                IsHitTestVisible = false,
            };
            Canvas.SetLeft(circle, random.NextDouble() * width - size / 2);
            Canvas.SetTop(circle, random.NextDouble() * height - size / 2);
            canvas.Children.Add(circle);
        }
    }

    private static void AddSunset(Canvas canvas, double width, double height)
    {
        AddGlow(canvas, width * 0.72, height * 0.20, 230, Current.DecorationPrimary, 0.40);
        AddGlow(canvas, width * 0.72, height * 0.20, 110, Current.DecorationPrimary, 0.55);

        var random = new Random(533);
        for (var i = 0; i < 5; i++)
        {
            var haze = new Ellipse
            {
                Width = width * (0.30 + random.NextDouble() * 0.45),
                Height = 40 + random.NextDouble() * 70,
                Fill = GlowBrush(Current.DecorationSecondary, 0.35),
                Opacity = 0.18 + random.NextDouble() * 0.18,
                IsHitTestVisible = false,
            };
            Canvas.SetLeft(haze, random.NextDouble() * width);
            Canvas.SetTop(haze, height * (0.62 + random.NextDouble() * 0.30));
            canvas.Children.Add(haze);
        }
    }

    private static void AddLeaves(Canvas canvas, double width, double height)
    {
        var random = new Random(4401);
        for (var i = 0; i < 16; i++)
        {
            var w = 18 + random.NextDouble() * 42;
            var h = 8 + random.NextDouble() * 20;
            var leaf = new Ellipse
            {
                Width = w,
                Height = h,
                Fill = Brush(i % 2 == 0 ? Current.DecorationPrimary : Current.DecorationSecondary),
                Opacity = 0.08 + random.NextDouble() * 0.13,
                IsHitTestVisible = false,
                RenderTransform = new RotateTransform(random.NextDouble() * 180),
                RenderTransformOrigin = new Point(0.5, 0.5),
            };

            var left = random.NextDouble() < 0.6
                ? random.NextDouble() * width * 0.35
                : width * 0.65 + random.NextDouble() * width * 0.35;
            Canvas.SetLeft(leaf, left);
            Canvas.SetTop(leaf, height * (0.12 + random.NextDouble() * 0.82));
            canvas.Children.Add(leaf);
        }

        AddGlow(canvas, width * 0.12, height * 0.78, 220, Current.DecorationPrimary, 0.08);
    }

    private static void AddGrid(Canvas canvas, double width, double height)
    {
        var stroke = Brush(Current.DecorationPrimary);
        stroke.Opacity = 0.055;
        const double step = 64;

        for (var x = step; x < width; x += step)
        {
            canvas.Children.Add(new Line
            {
                X1 = x,
                X2 = x,
                Y1 = 0,
                Y2 = height,
                Stroke = stroke,
                StrokeThickness = 1,
                IsHitTestVisible = false,
            });
        }

        for (var y = step; y < height; y += step)
        {
            canvas.Children.Add(new Line
            {
                X1 = 0,
                X2 = width,
                Y1 = y,
                Y2 = y,
                Stroke = stroke,
                StrokeThickness = 1,
                IsHitTestVisible = false,
            });
        }

        var ring = new Ellipse
        {
            Width = Math.Min(width, height) * 0.56,
            Height = Math.Min(width, height) * 0.56,
            Stroke = Brush(Current.DecorationPrimary, 0.10),
            StrokeThickness = 1,
            IsHitTestVisible = false,
        };
        Canvas.SetLeft(ring, width * 0.70);
        Canvas.SetTop(ring, height * 0.16);
        canvas.Children.Add(ring);
    }

    private static void AddPetals(Canvas canvas, double width, double height)
    {
        var random = new Random(7788);
        for (var i = 0; i < 26; i++)
        {
            var w = 5 + random.NextDouble() * 8;
            var h = 3 + random.NextDouble() * 5;
            var petal = new Ellipse
            {
                Width = w,
                Height = h,
                Fill = Brush(Current.DecorationPrimary),
                Opacity = 0.18 + random.NextDouble() * 0.24,
                IsHitTestVisible = false,
                RenderTransform = new RotateTransform(random.NextDouble() * 360),
                RenderTransformOrigin = new Point(0.5, 0.5),
            };
            Canvas.SetLeft(petal, random.NextDouble() * width);
            Canvas.SetTop(petal, random.NextDouble() * height);
            canvas.Children.Add(petal);

            var transform = new TranslateTransform();
            petal.RenderTransform = new TransformGroup
            {
                Children =
                {
                    new RotateTransform(random.NextDouble() * 360),
                    transform,
                },
            };
            transform.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation
            {
                From = -6,
                To = 6,
                Duration = TimeSpan.FromSeconds(5 + random.NextDouble() * 7),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
            });
        }
    }

    private static void AddGlow(Canvas canvas, double left, double top, double size, Color color, double opacity)
    {
        var glow = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = GlowBrush(color, 0.55),
            Opacity = opacity,
            IsHitTestVisible = false,
        };
        Canvas.SetLeft(glow, left - size / 2);
        Canvas.SetTop(glow, top - size / 2);
        canvas.Children.Add(glow);
    }

    private static void AddEllipse(Canvas canvas, double left, double top, double width, double height, Brush fill)
    {
        var ellipse = new Ellipse
        {
            Width = width,
            Height = height,
            Fill = fill,
            IsHitTestVisible = false,
        };
        Canvas.SetLeft(ellipse, left);
        Canvas.SetTop(ellipse, top);
        canvas.Children.Add(ellipse);
    }

    private static void Twinkle(UIElement element, double opacity, Random random)
    {
        element.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation
        {
            From = opacity * 0.22,
            To = opacity,
            Duration = TimeSpan.FromSeconds(1.2 + random.NextDouble() * 2.7),
            BeginTime = TimeSpan.FromSeconds(random.NextDouble() * 3),
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut },
        });
    }

    private static RadialGradientBrush GlowBrush(Color color, double centerOpacity)
    {
        var brush = new RadialGradientBrush
        {
            GradientOrigin = new Point(0.5, 0.5),
            Center = new Point(0.5, 0.5),
            RadiusX = 0.5,
            RadiusY = 0.5,
        };
        brush.GradientStops.Add(new GradientStop(
            Color.FromArgb((byte)Math.Clamp(centerOpacity * 255, 0, 255), color.R, color.G, color.B), 0));
        brush.GradientStops.Add(new GradientStop(
            Color.FromArgb((byte)Math.Clamp(centerOpacity * 100, 0, 255), color.R, color.G, color.B), 0.55));
        brush.GradientStops.Add(new GradientStop(
            Color.FromArgb(0, color.R, color.G, color.B), 1));
        return brush;
    }

    private static SolidColorBrush Brush(Color color, double opacity = 1.0)
    {
        return new SolidColorBrush(color) { Opacity = opacity };
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(
        IntPtr hwnd,
        int attribute,
        ref int value,
        int size);
}

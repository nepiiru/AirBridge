using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace AirBridge.App.Controls;

/// <summary>
/// Hosts the native HWND used by GStreamer's d3d11videosink.
/// WPF content cannot be drawn on top of this area (airspace limitation).
/// </summary>
public sealed class VideoHost : HwndHost
{
    private const int WsChild = 0x40000000;
    private const int WsVisible = 0x10000000;
    private const int WsClipChildren = 0x02000000;
    private const int WsClipSiblings = 0x04000000;
    private const int WsExNoActivate = 0x08000000;
    private const uint SwpNoZOrder = 0x0004;
    private const uint SwpNoActivate = 0x0010;
    private const string WindowClassName = "AirBridgeVideoHost";
    private const int BlackBrushIndex = 4;

    private static bool _windowClassRegistered;
    private static readonly WndProcDelegate DefWindowProcDelegate = DefWindowProc;

    private IntPtr _hwnd;

    public event EventHandler? HandleCreated;

    public new IntPtr Handle => _hwnd;

    private int SafeDimension(double value) =>
        double.IsNaN(value) || double.IsInfinity(value) ? 1 : Math.Max(1, (int)Math.Round(value));

    private static void EnsureWindowClass()
    {
        if (_windowClassRegistered)
        {
            return;
        }

        var windowClass = new WindowClass
        {
            Size = Marshal.SizeOf<WindowClass>(),
            WindowProc = DefWindowProcDelegate,
            Instance = GetModuleHandle(null),
            BackgroundBrush = GetStockObject(BlackBrushIndex),
            ClassName = WindowClassName,
        };

        RegisterClassEx(ref windowClass);
        _windowClassRegistered = true;
    }

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        EnsureWindowClass();

        _hwnd = CreateWindowEx(
            WsExNoActivate,
            WindowClassName,
            string.Empty,
            WsChild | WsVisible | WsClipChildren | WsClipSiblings,
            0,
            0,
            SafeDimension(ActualWidth),
            SafeDimension(ActualHeight),
            hwndParent.Handle,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero);

        if (_hwnd == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        HandleCreated?.Invoke(this, EventArgs.Empty);
        return new HandleRef(this, _hwnd);
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
        if (hwnd.Handle != IntPtr.Zero)
        {
            DestroyWindow(hwnd.Handle);
        }
        _hwnd = IntPtr.Zero;
    }

    protected override void OnWindowPositionChanged(Rect rcBoundingBox)
    {
        base.OnWindowPositionChanged(rcBoundingBox);

        if (_hwnd == IntPtr.Zero)
        {
            return;
        }

        var scale = PresentationSource.FromVisual(this)?.CompositionTarget?.TransformToDevice
                    ?? Matrix.Identity;
        var width = Math.Max(1, (int)Math.Round(rcBoundingBox.Width * scale.M11));
        var height = Math.Max(1, (int)Math.Round(rcBoundingBox.Height * scale.M22));

        SetWindowPos(_hwnd, IntPtr.Zero, 0, 0, width, height, SwpNoZOrder | SwpNoActivate);
    }

    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WindowClass
    {
        public int Size;
        public uint Style;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public WndProcDelegate WindowProc;
        public int ClassExtra;
        public int WindowExtra;
        public IntPtr Instance;
        public IntPtr Icon;
        public IntPtr Cursor;
        public IntPtr BackgroundBrush;
        public string? MenuName;
        public string ClassName;
        public IntPtr IconSmall;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern ushort RegisterClassEx(ref WindowClass windowClass);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr GetModuleHandle(string? moduleName);

    [DllImport("gdi32.dll")]
    private static extern IntPtr GetStockObject(int objectIndex);

    [DllImport("user32.dll")]
    private static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateWindowEx(
        int dwExStyle,
        string lpClassName,
        string lpWindowName,
        int dwStyle,
        int x,
        int y,
        int nWidth,
        int nHeight,
        IntPtr hWndParent,
        IntPtr hMenu,
        IntPtr hInstance,
        IntPtr lpParam);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int x,
        int y,
        int cx,
        int cy,
        uint uFlags);
}

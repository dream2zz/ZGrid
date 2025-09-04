using System;
using System.ComponentModel;
using System.Collections.Generic;
using ZGrid.Models;

namespace ZGrid.Models;

public class MySettings
{
    [Category("General"), DisplayName("User Name"), Description("The display name of the current user used across the app, shown in headers, logs, and recent file metadata.")]
    public string? UserName { get; set; } = Environment.UserName;

    [Category("General"), DisplayName("Enable Feature"), Description("Toggle a boolean feature flag.")]
    public bool EnableFeature { get; set; } = true;

    [Category("General"), DisplayName("Language"), Description("Preferred UI language (IETF tag).")]
    public string Language { get; set; } = "en-US";

    [Category("General"), DisplayName("Theme"), Description("Application theme.")]
    public Theme Theme { get; set; } = Theme.Default;

    [Category("General"), DisplayName("Startup Page"), Description("Default page to open on startup.")]
    public string StartupPage { get; set; } = "Home";

    [Category("General"), DisplayName("Auto Save"), Description("Automatically save changes.")]
    public bool AutoSave { get; set; } = true;

    [Category("General"), DisplayName("Auto Save Interval (min)"), Description("Interval in minutes for auto save.")]
    public int AutoSaveInterval { get; set; } = 10;

    // A demo for inline 3-level cascader editor
    [Category("Advanced"), DisplayName("Cascader Value"), Description("Select value with 3-level inline selector (pushes content below)."), CascaderEditor]
    public string CascaderValue { get; set; } = string.Empty;

    [Category("Advanced"), DisplayName("Retry Count"), Description("Number of retries for operation.")]
    public int RetryCount { get; set; } = 3;


    [Category("Advanced"), DisplayName("Log Level"), Description("Select log level.")]
    public LogLevel LogLevel { get; set; } = LogLevel.Info;

    [Category("Advanced"), DisplayName("Use Caching"), Description("Enable in-memory cache.")]
    public bool UseCaching { get; set; } = true;

    [Category("Advanced"), DisplayName("Cache Size (MB)"), Description("Maximum cache size in megabytes.")]
    public int CacheSizeMB { get; set; } = 256;

    [Category("Advanced"), DisplayName("Max Parallelism"), Description("Maximum degree of parallelism.")]
    public int Parallelism { get; set; } = 4;

    [Category("Advanced"), DisplayName("Diagnostics Enabled"), Description("Enable diagnostics collection.")]
    public bool DiagnosticsEnabled { get; set; } = false;

    [Category("Advanced"), DisplayName("Verbosity"), Description("Secondary verbosity level.")]
    public LogLevel Verbosity { get; set; } = LogLevel.Debug;

    [Category("Advanced"), DisplayName("Retry Backoff (ms)"), Description("Delay between retry attempts in milliseconds.")]
    public int RetryBackoffMs { get; set; } = 500;

    [Category("Advanced"), DisplayName("Experimental Mode"), Description("Enable experimental features.")]
    public bool ExperimentalMode { get; set; } = false;

    [Category("Design"), DisplayName("Accent Color"), Description("Primary accent color (hex). This is a deliberately long description to demonstrate wrapping and spacing of text in the panel; it is about one hundred characters long.")]
    public string AccentColor { get; set; } = "#0078d4";

    [Category("Design"), DisplayName("Font Family"), Description("Default font family name.")]
    public string FontFamily { get; set; } = "Segoe UI";

    [Category("Design"), DisplayName("Font Size"), Description("Default font size in points.")]
    public double FontSize { get; set; } = 12;

    [Category("Design"), DisplayName("Use Animations"), Description("Enable UI animations.")]
    public bool UseAnimations { get; set; } = true;

    [Category("Design"), DisplayName("Corner Radius"), Description("Corner radius in pixels.")]
    public int CornerRadius { get; set; } = 4;

    [Category("Design"), DisplayName("Opacity"), Description("Window opacity (0-1).")]
    public double Opacity { get; set; } = 1.0;

    [Category("Design"), DisplayName("High DPI Mode"), Description("Enable High-DPI rendering.")]
    public bool HighDpiMode { get; set; } = true;

    [Category("Misc"), DisplayName("Notes"), Description("Free form notes.")]
    public string? Notes { get; set; }

    [Category("Misc"), DisplayName("Tags"), Description("Comma separated tags.")]
    public string Tags { get; set; } = string.Empty;

    [Category("Misc"), DisplayName("Remember Recent"), Description("Remember recent files.")]
    public bool RememberRecent { get; set; } = true;

    [Category("Misc"), DisplayName("Temp Folder"), Description("Temporary folder path.")]
    public string TempFolder { get; set; } = System.IO.Path.GetTempPath();

    [Category("Misc"), DisplayName("Session Id"), Description("Current session id.")]
    public string SessionId { get; set; } = Guid.NewGuid().ToString("N");

    [Category("Layout"), DisplayName("Window Width"), Description("Window width in pixels.")]
    public int WindowWidth { get; set; } = 1024;

    [Category("Layout"), DisplayName("Window Height"), Description("Window height in pixels.")]
    public int WindowHeight { get; set; } = 768;

    [Category("Layout"), DisplayName("Sidebar Visible"), Description("Show sidebar.")]
    public bool SidebarVisible { get; set; } = true;

    [Category("Layout"), DisplayName("Toolbar Visible"), Description("Show toolbar.")]
    public bool ToolbarVisible { get; set; } = true;

    [Category("Layout"), DisplayName("Splitter Distance"), Description("Splitter distance in pixels.")]
    public int SplitterDistance { get; set; } = 240;

    [Category("Layout"), DisplayName("Show Grid Lines"), Description("Show grid lines in tables.")]
    public bool GridLines { get; set; } = false;

    [Category("Layout"), DisplayName("Density"), Description("UI density.")]
    public Density Density { get; set; } = Density.Comfortable;

    [Category("Window"), DisplayName("Window State"), Description("Initial window state.")]
    public AppWindowState WindowState { get; set; } = AppWindowState.Normal;

    [Category("Window"), DisplayName("Start Position"), Description("Initial window start position.")]
    public StartPosition StartPosition { get; set; } = StartPosition.WindowsDefaultLocation;

    [Category("Window"), DisplayName("Always On Top"), Description("Keep window on top.")]
    public bool AlwaysOnTop { get; set; } = false;

    [Category("Window"), DisplayName("Show In Taskbar"), Description("Show window in taskbar.")]
    public bool ShowInTaskbar { get; set; } = true;

    [Category("Window"), DisplayName("Transparency"), Description("Enable window transparency.")]
    public bool Transparency { get; set; } = false;

    [Category("Window"), DisplayName("Border Style"), Description("Window border style.")]
    public WindowBorderStyle BorderStyle { get; set; } = WindowBorderStyle.Resizable;

    [Category("Behavior"), DisplayName("Confirm On Exit"), Description("Ask for confirmation when exiting.")]
    public bool ConfirmOnExit { get; set; } = true;

    [Category("Behavior"), DisplayName("Enable Shortcuts"), Description("Enable keyboard shortcuts.")]
    public bool EnableShortcuts { get; set; } = true;

    [Category("Behavior"), DisplayName("Drag & Drop"), Description("Enable drag and drop.")]
    public bool DragDropEnabled { get; set; } = true;

    [Category("Behavior"), DisplayName("Auto Reload Files"), Description("Auto reload files when changed.")]
    public bool AutoReloadFiles { get; set; } = false;

    [Category("Text"), DisplayName("Title"), Description("Application title.")]
    public string Title { get; set; } = "ZGrid";

    [Category("Text"), DisplayName("Description"), Description("Sample settings - this description is intentionally long to test the panel sizing, wrapping and scrolling behavior within the property grid UI.")]
    public string Description { get; set; } = "Sample settings";

    [Category("Text"), DisplayName("Watermark"), Description("Watermark text.")]
    public string Watermark { get; set; } = string.Empty;
}

public enum LogLevel
{
    Trace,
    Debug,
    Info,
    Warn,
    Error,
    Fatal
}

public enum Theme
{
    Default,
    Light,
    Dark,
}

public enum Density
{
    Compact,
    Comfortable,
    Spacious
}

public enum AppWindowState
{
    Normal,
    Maximized,
    Minimized
}

public enum StartPosition
{
    Manual,
    CenterScreen,
    WindowsDefaultLocation
}

public enum WindowBorderStyle
{
    None,
    Single,
    Resizable
}
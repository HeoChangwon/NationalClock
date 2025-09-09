using System.Text.Json.Serialization;

namespace NationalClock.Models;

/// <summary>
/// Application settings model with JSON serialization support
/// Contains all user preferences and configuration options
/// </summary>
public class Settings
{
    /// <summary>
    /// Whether to use 24-hour time format (true) or 12-hour format (false)
    /// </summary>
    public bool Is24HourFormat { get; set; } = true;

    /// <summary>
    /// Whether to use dark theme (true) or light theme (false)
    /// </summary>
    public bool IsDarkMode { get; set; } = false;

    /// <summary>
    /// Whether the main window should always stay on top
    /// </summary>
    public bool IsAlwaysOnTop { get; set; } = false;

    /// <summary>
    /// Main window position X coordinate
    /// </summary>
    public double WindowLeft { get; set; } = 100;

    /// <summary>
    /// Main window position Y coordinate
    /// </summary>
    public double WindowTop { get; set; } = 100;

    /// <summary>
    /// Main window width
    /// </summary>
    public double WindowWidth { get; set; } = 800;

    /// <summary>
    /// Main window height
    /// </summary>
    public double WindowHeight { get; set; } = 600;

    /// <summary>
    /// Whether the main window is maximized
    /// </summary>
    public bool IsWindowMaximized { get; set; } = false;

    /// <summary>
    /// List of enabled timezone IDs in display order
    /// </summary>
    public List<int> EnabledTimeZoneIds { get; set; } = new();

    /// <summary>
    /// Auto-start with Windows (for future implementation)
    /// </summary>
    public bool AutoStartWithWindows { get; set; } = false;

    /// <summary>
    /// Show seconds in time display
    /// </summary>
    public bool ShowSeconds { get; set; } = true;

    /// <summary>
    /// Show date below time
    /// </summary>
    public bool ShowDate { get; set; } = true;

    /// <summary>
    /// Show timezone offset
    /// </summary>
    public bool ShowTimeZoneOffset { get; set; } = true;

    /// <summary>
    /// Compact mode for smaller display
    /// </summary>
    public bool IsCompactMode { get; set; } = false;

    /// <summary>
    /// Theme color accent (Material Design primary color)
    /// </summary>
    public string ThemeAccentColor { get; set; } = "Blue";

    /// <summary>
    /// Update interval in seconds (minimum 1 second)
    /// </summary>
    public int UpdateIntervalSeconds { get; set; } = 1;

    /// <summary>
    /// Language/Culture setting (for future localization)
    /// </summary>
    public string Culture { get; set; } = "en-US";

    /// <summary>
    /// Settings file version for compatibility checking
    /// </summary>
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// Last saved timestamp
    /// </summary>
    public DateTime LastSaved { get; set; } = DateTime.Now;

    /// <summary>
    /// Default constructor that initializes with default values
    /// </summary>
    public Settings()
    {
        // Initialize with default enabled timezones (Korea, Michigan, Poland)
        EnabledTimeZoneIds = new List<int> { 1, 2, 3 };
    }

    /// <summary>
    /// Creates a copy of the current settings
    /// </summary>
    /// <returns>Deep copy of settings</returns>
    public Settings Clone()
    {
        return new Settings
        {
            Is24HourFormat = Is24HourFormat,
            IsDarkMode = IsDarkMode,
            IsAlwaysOnTop = IsAlwaysOnTop,
            WindowLeft = WindowLeft,
            WindowTop = WindowTop,
            WindowWidth = WindowWidth,
            WindowHeight = WindowHeight,
            IsWindowMaximized = IsWindowMaximized,
            EnabledTimeZoneIds = new List<int>(EnabledTimeZoneIds),
            AutoStartWithWindows = AutoStartWithWindows,
            ShowSeconds = ShowSeconds,
            ShowDate = ShowDate,
            ShowTimeZoneOffset = ShowTimeZoneOffset,
            IsCompactMode = IsCompactMode,
            ThemeAccentColor = ThemeAccentColor,
            UpdateIntervalSeconds = UpdateIntervalSeconds,
            Culture = Culture,
            Version = Version,
            LastSaved = DateTime.Now
        };
    }

    /// <summary>
    /// Validates and fixes any invalid settings values
    /// </summary>
    public void ValidateAndFix()
    {
        // Ensure update interval is at least 1 second
        if (UpdateIntervalSeconds < 1)
            UpdateIntervalSeconds = 1;

        // Ensure window dimensions are reasonable
        if (WindowWidth < 300)
            WindowWidth = 300;
        if (WindowHeight < 200)
            WindowHeight = 200;

        // Ensure window position is not completely off-screen
        if (WindowLeft < -1000)
            WindowLeft = 100;
        if (WindowTop < -1000)
            WindowTop = 100;

        // Ensure enabled timezone list is not null
        EnabledTimeZoneIds ??= new List<int> { 1, 2, 3 };

        // Update last saved timestamp
        LastSaved = DateTime.Now;
    }

    /// <summary>
    /// Gets a display-friendly string representation of the settings
    /// </summary>
    public override string ToString()
    {
        return $"Settings: {(Is24HourFormat ? "24H" : "12H")}, " +
               $"{(IsDarkMode ? "Dark" : "Light")} theme, " +
               $"{EnabledTimeZoneIds.Count} timezones enabled";
    }
}
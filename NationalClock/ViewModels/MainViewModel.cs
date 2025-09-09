using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NationalClock.Models;
using NationalClock.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace NationalClock.ViewModels;

/// <summary>
/// Main ViewModel for the National Clock application
/// Manages clock display, format settings, theme settings, and window behavior
/// </summary>
public partial class MainViewModel : BaseViewModel, IDisposable
{
    private readonly ClockService _clockService;
    private readonly TimeZoneManager _timeZoneManager;
    private readonly ThemeManager _themeManager;
    private readonly SettingsManager _settingsManager;
    private bool _disposed = false;

    [ObservableProperty]
    private bool _is24HourFormat = true;

    [ObservableProperty]
    private bool _isDarkMode = false;

    [ObservableProperty]
    private bool _isAlwaysOnTop = false;

    [ObservableProperty]
    private string _windowTitle = "National Clock";

    /// <summary>
    /// Constructor with dependency injection of required services
    /// </summary>
    /// <param name="clockService">Service for managing clock operations</param>
    /// <param name="timeZoneManager">Service for managing timezone data</param>
    /// <param name="themeManager">Service for managing Material Design themes</param>
    /// <param name="settingsManager">Service for managing application settings</param>
    public MainViewModel(
        ClockService clockService,
        TimeZoneManager timeZoneManager,
        ThemeManager themeManager,
        SettingsManager settingsManager)
    {
        _clockService = clockService ?? ClockService.Instance;
        _timeZoneManager = timeZoneManager ?? TimeZoneManager.Instance;
        _themeManager = themeManager ?? ThemeManager.Instance;
        _settingsManager = settingsManager ?? SettingsManager.Instance;

        // Initialize from current settings
        InitializeFromSettings();

        // Subscribe to service events
        _clockService.TimeFormatChanged += OnTimeFormatChanged;
        _themeManager.ThemeChanged += OnThemeChanged;
        _settingsManager.SettingsChanged += OnSettingsChanged;

        // Start the clock service
        _clockService.Start();
    }

    /// <summary>
    /// Gets the observable collection of clocks from the ClockService
    /// </summary>
    public ObservableCollection<ClockInfo> Clocks => _clockService.Clocks;

    /// <summary>
    /// Command to toggle between 12-hour and 24-hour time format
    /// </summary>
    [RelayCommand]
    private void ToggleFormat()
    {
        try
        {
            Is24HourFormat = !Is24HourFormat;
            _clockService.Is24HourFormat = Is24HourFormat;

            // Save setting
            _settingsManager.UpdateSetting(s => s.Is24HourFormat = Is24HourFormat);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error toggling format: {ex.Message}");
        }
    }

    /// <summary>
    /// Command to open the settings window
    /// </summary>
    [RelayCommand]
    private void OpenSettings()
    {
        try
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var result = Views.SettingsWindow.ShowSettingsDialog(Application.Current.MainWindow);
                
                if (result == true)
                {
                    // Settings were saved, refresh clocks to apply changes
                    _clockService.RefreshClocks();
                    UpdateWindowTitle();
                    System.Diagnostics.Debug.WriteLine("Settings saved and applied");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Settings cancelled or not saved");
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error opening settings: {ex.Message}");
        }
    }

    /// <summary>
    /// Command to add a timezone quickly (shows a simple selection)
    /// </summary>
    [RelayCommand]
    private void AddTimeZone()
    {
        try
        {
            // Get first available disabled timezone
            var availableTimeZone = _timeZoneManager.AllTimeZones
                .FirstOrDefault(tz => !tz.IsEnabled);

            if (availableTimeZone != null)
            {
                // Enable the timezone
                _timeZoneManager.SetTimeZoneEnabled(availableTimeZone.Id, true);
                
                // Update settings
                var enabledIds = _timeZoneManager.EnabledTimeZones.Select(tz => tz.Id).ToList();
                _settingsManager.UpdateSetting(s => s.EnabledTimeZoneIds = enabledIds);

                // Refresh clocks
                _clockService.RefreshClocks();

                System.Diagnostics.Debug.WriteLine($"Added timezone: {availableTimeZone.DisplayName}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No more timezones available to add");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error adding timezone: {ex.Message}");
        }
    }

    /// <summary>
    /// Command to toggle between dark and light theme
    /// </summary>
    [RelayCommand]
    private void ToggleTheme()
    {
        try
        {
            IsDarkMode = !IsDarkMode;
            _themeManager.IsDarkMode = IsDarkMode;

            // Save setting
            _settingsManager.UpdateSetting(s => s.IsDarkMode = IsDarkMode);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error toggling theme: {ex.Message}");
        }
    }

    /// <summary>
    /// Command to toggle the always-on-top window behavior
    /// </summary>
    [RelayCommand]
    private void ToggleAlwaysOnTop()
    {
        try
        {
            IsAlwaysOnTop = !IsAlwaysOnTop;

            // Save setting
            _settingsManager.UpdateSetting(s => s.IsAlwaysOnTop = IsAlwaysOnTop);

            // Apply to main window
            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.Topmost = IsAlwaysOnTop;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error toggling always on top: {ex.Message}");
        }
    }

    /// <summary>
    /// Command to refresh all clocks manually
    /// </summary>
    [RelayCommand]
    private void RefreshClocks()
    {
        try
        {
            _clockService.RefreshClocks();
            _clockService.ForceUpdate();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error refreshing clocks: {ex.Message}");
        }
    }

    /// <summary>
    /// Command to exit the application
    /// </summary>
    [RelayCommand]
    private void ExitApplication()
    {
        try
        {
            // Stop the clock service
            _clockService.Stop();

            // Save current window settings if available
            SaveWindowSettings();

            // Exit the application
            Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error exiting application: {ex.Message}");
        }
    }

    /// <summary>
    /// Initializes ViewModel properties from current settings
    /// </summary>
    private void InitializeFromSettings()
    {
        try
        {
            var settings = _settingsManager.CurrentSettings;
            
            Is24HourFormat = settings.Is24HourFormat;
            IsDarkMode = settings.IsDarkMode;
            IsAlwaysOnTop = settings.IsAlwaysOnTop;

            // Apply clock format
            _clockService.Is24HourFormat = Is24HourFormat;
            
            // Apply theme
            _themeManager.IsDarkMode = IsDarkMode;

            // Update window title based on enabled clocks count
            UpdateWindowTitle();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing from settings: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates the window title based on the number of enabled clocks
    /// </summary>
    private void UpdateWindowTitle()
    {
        try
        {
            var enabledCount = _timeZoneManager.EnabledTimeZones.Count();
            WindowTitle = enabledCount > 0 
                ? $"National Clock ({enabledCount} {(enabledCount == 1 ? "clock" : "clocks")})"
                : "National Clock";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating window title: {ex.Message}");
            WindowTitle = "National Clock";
        }
    }

    /// <summary>
    /// Saves current window position and size to settings
    /// </summary>
    private void SaveWindowSettings()
    {
        try
        {
            if (Application.Current.MainWindow != null)
            {
                var window = Application.Current.MainWindow;
                
                _settingsManager.UpdateSetting(s =>
                {
                    if (window.WindowState == WindowState.Normal)
                    {
                        s.WindowLeft = window.Left;
                        s.WindowTop = window.Top;
                        s.WindowWidth = window.Width;
                        s.WindowHeight = window.Height;
                        s.IsWindowMaximized = false;
                    }
                    else
                    {
                        s.IsWindowMaximized = window.WindowState == WindowState.Maximized;
                    }
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving window settings: {ex.Message}");
        }
    }

    /// <summary>
    /// Event handler for ClockService time format changes
    /// </summary>
    private void OnTimeFormatChanged(object? sender, bool is24Hour)
    {
        if (Is24HourFormat != is24Hour)
        {
            Is24HourFormat = is24Hour;
        }
    }

    /// <summary>
    /// Event handler for ThemeManager theme changes
    /// </summary>
    private void OnThemeChanged(object? sender, bool isDark)
    {
        if (IsDarkMode != isDark)
        {
            IsDarkMode = isDark;
        }
    }

    /// <summary>
    /// Event handler for SettingsManager settings changes
    /// </summary>
    private void OnSettingsChanged(object? sender, Settings settings)
    {
        try
        {
            // Update properties if they've changed externally
            if (Is24HourFormat != settings.Is24HourFormat)
                Is24HourFormat = settings.Is24HourFormat;
            
            if (IsDarkMode != settings.IsDarkMode)
                IsDarkMode = settings.IsDarkMode;
            
            if (IsAlwaysOnTop != settings.IsAlwaysOnTop)
                IsAlwaysOnTop = settings.IsAlwaysOnTop;

            // Update window title
            UpdateWindowTitle();

            // Apply always on top to window if available
            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.Topmost = IsAlwaysOnTop;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error handling settings change: {ex.Message}");
        }
    }

    /// <summary>
    /// Disposes of resources and unsubscribes from events
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            try
            {
                // Unsubscribe from events
                _clockService.TimeFormatChanged -= OnTimeFormatChanged;
                _themeManager.ThemeChanged -= OnThemeChanged;
                _settingsManager.SettingsChanged -= OnSettingsChanged;

                // Stop clock service
                _clockService.Stop();

                // Save final window settings
                SaveWindowSettings();

                _disposed = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error disposing MainViewModel: {ex.Message}");
            }
        }
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer to ensure resources are cleaned up
    /// </summary>
    ~MainViewModel()
    {
        Dispose();
    }
}
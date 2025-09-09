using System.ComponentModel;
using System.Windows;
using NationalClock.ViewModels;
using NationalClock.Services;

namespace NationalClock;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly SettingsManager _settingsManager;

    public MainWindow()
    {
        InitializeComponent();
        
        // Initialize services
        _settingsManager = SettingsManager.Instance;
        
        // Initialize ViewModel with services
        _viewModel = new MainViewModel(
            ClockService.Instance,
            TimeZoneManager.Instance, 
            ThemeManager.Instance,
            _settingsManager);
        
        // Set DataContext
        DataContext = _viewModel;
        
        // Load and apply window settings
        LoadWindowSettings();
        
        // Subscribe to theme changes for dynamic updates
        ThemeManager.Instance.ThemeChanged += OnThemeChanged;
    }

    /// <summary>
    /// Loads window position, size, and state from settings
    /// </summary>
    private void LoadWindowSettings()
    {
        try
        {
            var settings = _settingsManager.CurrentSettings;
            
            // Restore window position and size
            if (settings.WindowLeft >= 0 && settings.WindowTop >= 0)
            {
                Left = settings.WindowLeft;
                Top = settings.WindowTop;
            }
            
            if (settings.WindowWidth > 0 && settings.WindowHeight > 0)
            {
                Width = Math.Max(settings.WindowWidth, MinWidth);
                Height = Math.Max(settings.WindowHeight, MinHeight);
            }
            
            // Restore window state
            if (settings.IsWindowMaximized)
            {
                WindowState = WindowState.Maximized;
            }
            
            // Apply always on top setting
            Topmost = settings.IsAlwaysOnTop;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading window settings: {ex.Message}");
        }
    }

    /// <summary>
    /// Saves current window position and size to settings
    /// </summary>
    private void SaveWindowSettings()
    {
        try
        {
            _settingsManager.UpdateSetting(s =>
            {
                if (WindowState == WindowState.Normal)
                {
                    s.WindowLeft = Left;
                    s.WindowTop = Top;
                    s.WindowWidth = Width;
                    s.WindowHeight = Height;
                    s.IsWindowMaximized = false;
                }
                else
                {
                    s.IsWindowMaximized = WindowState == WindowState.Maximized;
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving window settings: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles theme changes to update window appearance
    /// </summary>
    private void OnThemeChanged(object? sender, bool isDarkMode)
    {
        try
        {
            // Simple theme switching without Material Design
            if (isDarkMode)
            {
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(48, 48, 48));
            }
            else
            {
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating theme: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles window closing event
    /// </summary>
    protected override void OnClosing(CancelEventArgs e)
    {
        try
        {
            // Save window settings before closing
            SaveWindowSettings();
            
            // Dispose ViewModel resources
            _viewModel?.Dispose();
            
            // Unsubscribe from events
            ThemeManager.Instance.ThemeChanged -= OnThemeChanged;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during window closing: {ex.Message}");
        }
        
        base.OnClosing(e);
    }

    /// <summary>
    /// Handles window state changes to save settings
    /// </summary>
    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);
        
        // Save window state changes
        if (IsLoaded)
        {
            SaveWindowSettings();
        }
    }

    /// <summary>
    /// Handles window location changes to save settings
    /// </summary>
    protected override void OnLocationChanged(EventArgs e)
    {
        base.OnLocationChanged(e);
        
        // Save location changes
        if (IsLoaded && WindowState == WindowState.Normal)
        {
            SaveWindowSettings();
        }
    }

    /// <summary>
    /// Handles window size changes to save settings
    /// </summary>
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        
        // Save size changes
        if (IsLoaded && WindowState == WindowState.Normal)
        {
            SaveWindowSettings();
        }
    }

    /// <summary>
    /// Handles window loaded event to ensure proper initialization
    /// </summary>
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        
        try
        {
            // Ensure window is positioned correctly on the screen
            EnsureWindowOnScreen();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in source initialized: {ex.Message}");
        }
    }

    /// <summary>
    /// Ensures the window is positioned within visible screen bounds
    /// </summary>
    private void EnsureWindowOnScreen()
    {
        try
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            
            // Ensure window is not positioned off-screen
            if (Left + Width > screenWidth)
            {
                Left = Math.Max(0, screenWidth - Width);
            }
            
            if (Top + Height > screenHeight)
            {
                Top = Math.Max(0, screenHeight - Height);
            }
            
            if (Left < 0) Left = 0;
            if (Top < 0) Top = 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error ensuring window on screen: {ex.Message}");
        }
    }
}
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
        System.Diagnostics.Debug.WriteLine("MainWindow: Constructor started");
        
        InitializeComponent();
        
        System.Diagnostics.Debug.WriteLine($"MainWindow: After InitializeComponent - Position: {Left}, {Top}");
        
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
        
        // Subscribe to theme changes for dynamic updates
        ThemeManager.Instance.ThemeChanged += OnThemeChanged;
        
        System.Diagnostics.Debug.WriteLine($"MainWindow: Before LoadWindowSettings - Position: {Left}, {Top}");
        
        // Load and apply window settings after ViewModel is initialized
        LoadWindowSettings();
        
        System.Diagnostics.Debug.WriteLine($"MainWindow: After LoadWindowSettings - Position: {Left}, {Top}");
        System.Diagnostics.Debug.WriteLine("MainWindow: Constructor completed");
    }

    /// <summary>
    /// Loads window position, size, and state from settings
    /// </summary>
    private void LoadWindowSettings()
    {
        try
        {
            var settings = _settingsManager.CurrentSettings;
            
            System.Diagnostics.Debug.WriteLine($"MainWindow.LoadWindowSettings: Loading window settings");
            System.Diagnostics.Debug.WriteLine($"MainWindow.LoadWindowSettings: Settings position: {settings.WindowLeft}, {settings.WindowTop}");
            System.Diagnostics.Debug.WriteLine($"MainWindow.LoadWindowSettings: Settings size: {settings.WindowWidth}x{settings.WindowHeight}");
            System.Diagnostics.Debug.WriteLine($"MainWindow.LoadWindowSettings: Current position before: {Left}, {Top}");
            
            // Restore window position and size
            if (settings.WindowLeft >= 0 && settings.WindowTop >= 0)
            {
                Left = settings.WindowLeft;
                Top = settings.WindowTop;
                System.Diagnostics.Debug.WriteLine($"MainWindow.LoadWindowSettings: Applied position: {Left}, {Top}");
            }
            
            if (settings.WindowWidth > 0 && settings.WindowHeight > 0)
            {
                Width = Math.Max(settings.WindowWidth, MinWidth);
                Height = Math.Max(settings.WindowHeight, MinHeight);
                System.Diagnostics.Debug.WriteLine($"MainWindow.LoadWindowSettings: Applied size: {Width}x{Height}");
            }
            
            // Restore window state
            if (settings.IsWindowMaximized)
            {
                WindowState = WindowState.Maximized;
                System.Diagnostics.Debug.WriteLine($"MainWindow.LoadWindowSettings: Applied maximized state");
            }
            
            // Apply always on top setting
            Topmost = settings.IsAlwaysOnTop;
            System.Diagnostics.Debug.WriteLine($"MainWindow.LoadWindowSettings: Applied always on top: {Topmost}");
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
            System.Diagnostics.Debug.WriteLine($"MainWindow.OnSourceInitialized: Before EnsureWindowOnScreen - Position: {Left}, {Top}");
            // Ensure window is positioned correctly on the screen
            EnsureWindowOnScreen();
            System.Diagnostics.Debug.WriteLine($"MainWindow.OnSourceInitialized: After EnsureWindowOnScreen - Position: {Left}, {Top}");
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
            
            System.Diagnostics.Debug.WriteLine($"MainWindow.EnsureWindowOnScreen: Screen size: {screenWidth}x{screenHeight}");
            System.Diagnostics.Debug.WriteLine($"MainWindow.EnsureWindowOnScreen: Current position: {Left}, {Top}");
            System.Diagnostics.Debug.WriteLine($"MainWindow.EnsureWindowOnScreen: Current size: {Width}x{Height}");
            
            var originalLeft = Left;
            var originalTop = Top;
            
            // Ensure window is not positioned off-screen
            if (Left + Width > screenWidth)
            {
                Left = Math.Max(0, screenWidth - Width);
                System.Diagnostics.Debug.WriteLine($"MainWindow.EnsureWindowOnScreen: Adjusted Left from {originalLeft} to {Left} (too far right)");
            }
            
            if (Top + Height > screenHeight)
            {
                Top = Math.Max(0, screenHeight - Height);
                System.Diagnostics.Debug.WriteLine($"MainWindow.EnsureWindowOnScreen: Adjusted Top from {originalTop} to {Top} (too far down)");
            }
            
            if (Left < 0) 
            {
                System.Diagnostics.Debug.WriteLine($"MainWindow.EnsureWindowOnScreen: Adjusted Left from {Left} to 0 (negative)");
                Left = 0;
            }
            if (Top < 0) 
            {
                System.Diagnostics.Debug.WriteLine($"MainWindow.EnsureWindowOnScreen: Adjusted Top from {Top} to 0 (negative)");
                Top = 0;
            }
            
            if (originalLeft != Left || originalTop != Top)
            {
                System.Diagnostics.Debug.WriteLine($"MainWindow.EnsureWindowOnScreen: Position changed from ({originalLeft}, {originalTop}) to ({Left}, {Top})");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"MainWindow.EnsureWindowOnScreen: Position unchanged");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error ensuring window on screen: {ex.Message}");
        }
    }
}
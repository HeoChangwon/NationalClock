using System.Windows;
using System.Windows.Media;
using NationalClock.Services;
using MaterialDesignThemes.Wpf;

namespace NationalClock;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            // Fix for WPF stylus input handling issue
            DisableStylusAndTouch();
            
            // Set up global exception handlers
            SetupGlobalExceptionHandling();
            
            // Initialize Material Design theme first
            InitializeMaterialDesign();
            
            // Initialize services (this will apply theme settings)
            InitializeServices();
            
            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Application failed to start: {ex.Message}", 
                          "National Clock - Startup Error", 
                          MessageBoxButton.OK, 
                          MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    /// <summary>
    /// Disables stylus and touch input to prevent WPF input handling issues
    /// </summary>
    private static void DisableStylusAndTouch()
    {
        try
        {
            // Disable WPF stylus input
            AppContext.SetSwitch("Switch.System.Windows.Input.Stylus.DisableStylusAndTouchSupport", true);
            
            // Alternative method using reflection if the above doesn't work
            var stylusInputType = typeof(System.Windows.Input.StylusPlugIns.StylusPlugIn).Assembly
                .GetType("System.Windows.Input.Stylus");
            
            if (stylusInputType != null)
            {
                var disableProperty = stylusInputType.GetProperty("DisableStylusAndTouchSupport", 
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                
                disableProperty?.SetValue(null, true);
            }
        }
        catch
        {
            // Ignore errors - this is a best-effort fix
        }
    }

    /// <summary>
    /// Sets up global exception handling for the application
    /// </summary>
    private void SetupGlobalExceptionHandling()
    {
        // Handle unhandled exceptions in the UI thread
        this.DispatcherUnhandledException += (sender, e) =>
        {
            System.Diagnostics.Debug.WriteLine($"UI Exception: {e.Exception.Message}");
            
            // Show user-friendly error dialog
            var result = MessageBox.Show(
                $"An unexpected error occurred:\n\n{e.Exception.Message}\n\nWould you like to continue running the application?",
                "National Clock - Error",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            
            if (result == MessageBoxResult.Yes)
            {
                e.Handled = true; // Continue running
            }
            else
            {
                Shutdown(1); // Exit application
            }
        };

        // Handle unhandled exceptions in background threads
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            var exception = e.ExceptionObject as Exception;
            System.Diagnostics.Debug.WriteLine($"Background Exception: {exception?.Message}");
            
            if (!e.IsTerminating)
            {
                MessageBox.Show(
                    $"A background error occurred:\n\n{exception?.Message}",
                    "National Clock - Background Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        };

        // Handle task exceptions
        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            System.Diagnostics.Debug.WriteLine($"Task Exception: {e.Exception.Message}");
            
            // Mark as observed to prevent crash
            e.SetObserved();
            
            // Show warning to user
            MessageBox.Show(
                $"A background task error occurred:\n\n{e.Exception.InnerException?.Message ?? e.Exception.Message}",
                "National Clock - Task Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        };
    }

    /// <summary>
    /// Initializes application services
    /// </summary>
    private void InitializeServices()
    {
        try
        {
            // Initialize singleton services in proper order
            var settingsManager = SettingsManager.Instance;
            System.Diagnostics.Debug.WriteLine("App.InitializeServices: SettingsManager created");
            
            var themeManager = ThemeManager.Instance;
            System.Diagnostics.Debug.WriteLine("App.InitializeServices: ThemeManager created");
            
            var timeZoneManager = TimeZoneManager.Instance;
            System.Diagnostics.Debug.WriteLine("App.InitializeServices: TimeZoneManager created");
            
            var clockService = ClockService.Instance;
            System.Diagnostics.Debug.WriteLine("App.InitializeServices: ClockService created");

            // Settings will be loaded automatically by the manager
            // settingsManager.LoadSettings();

            // Apply initial theme
            var settings = settingsManager.CurrentSettings;
            System.Diagnostics.Debug.WriteLine($"App.InitializeServices: Loading settings - EnabledTimeZoneIds: [{string.Join(", ", settings.EnabledTimeZoneIds)}]");
            System.Diagnostics.Debug.WriteLine($"App.InitializeServices: Window position: {settings.WindowLeft}, {settings.WindowTop}");
            System.Diagnostics.Debug.WriteLine($"App.InitializeServices: Window size: {settings.WindowWidth}x{settings.WindowHeight}");
            
            System.Diagnostics.Debug.WriteLine($"App.InitializeServices: Theme settings - IsDarkMode: {settings.IsDarkMode}, AccentColor: {settings.ThemeAccentColor}");
            
            // Apply theme settings using the proper method
            themeManager.ApplySettingsTheme(settings);
            
            System.Diagnostics.Debug.WriteLine($"App.InitializeServices: Applied to ThemeManager - IsDarkMode: {themeManager.IsDarkMode}, AccentColor: {themeManager.CurrentAccentColor}");

            // Apply timezone settings from saved configuration
            timeZoneManager.UpdateEnabledTimeZones(settings.EnabledTimeZoneIds);
            System.Diagnostics.Debug.WriteLine($"App.InitializeServices: Applied timezone settings");

            System.Diagnostics.Debug.WriteLine("Services initialized successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing services: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Initializes Material Design theme system
    /// </summary>
    private void InitializeMaterialDesign()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("App.InitializeMaterialDesign: Setting up basic Material Design theme (settings will be applied later by ThemeManager)");
            
            // Just initialize Material Design with default settings
            // The actual theme will be applied by ThemeManager in InitializeServices()
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();

            // Set a basic light theme initially
            theme.SetBaseTheme(BaseTheme.Light);
            theme.SetPrimaryColor(Colors.Blue);

            // Apply basic theme
            paletteHelper.SetTheme(theme);

            System.Diagnostics.Debug.WriteLine("App.InitializeMaterialDesign: Basic Material Design theme initialized");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing Material Design: {ex.Message}");
            // Continue with default theme if initialization fails
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            // Settings are saved automatically by the manager
            // SettingsManager.Instance.SaveSettings();
            
            // Stop clock service
            ClockService.Instance.Stop();
            
            System.Diagnostics.Debug.WriteLine("Application shutdown complete");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during application shutdown: {ex.Message}");
        }
        
        base.OnExit(e);
    }

    /// <summary>
    /// Global exception handler
    /// </summary>
    protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
    {
        try
        {
            // Settings are saved automatically by the manager
            // SettingsManager.Instance.SaveSettings();
            ClockService.Instance.Stop();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during session ending: {ex.Message}");
        }
        
        base.OnSessionEnding(e);
    }
}


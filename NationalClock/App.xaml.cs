using System.Windows;
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
            // Set up global exception handlers
            SetupGlobalExceptionHandling();
            
            // Initialize services
            InitializeServices();
            
            // Initialize Material Design theme
            InitializeMaterialDesign();
            
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
            var themeManager = ThemeManager.Instance;
            var timeZoneManager = TimeZoneManager.Instance;
            var clockService = ClockService.Instance;

            // Settings will be loaded automatically by the manager
            // settingsManager.LoadSettings();

            // Apply initial theme
            var settings = settingsManager.CurrentSettings;
            themeManager.IsDarkMode = settings.IsDarkMode;
            themeManager.CurrentAccentColor = settings.ThemeAccentColor;

            // Initialize timezones - using public method
            // timeZoneManager.InitializeDefaultTimeZones();
            // Timezones should be initialized automatically by the manager

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
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();

            // Apply initial theme settings
            var settingsManager = SettingsManager.Instance;
            var settings = settingsManager.CurrentSettings;

            // Set base theme (dark/light)
            theme.SetBaseTheme(settings.IsDarkMode ? BaseTheme.Dark : BaseTheme.Light);
            
            // Set primary color based on accent color setting
            var primaryColor = ThemeManager.GetMaterialDesignColor(settings.ThemeAccentColor);
            if (primaryColor.HasValue)
            {
                theme.SetPrimaryColor(primaryColor.Value);
            }

            // Apply theme
            paletteHelper.SetTheme(theme);

            System.Diagnostics.Debug.WriteLine($"Material Design initialized - Dark: {settings.IsDarkMode}, Accent: {settings.ThemeAccentColor}");
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


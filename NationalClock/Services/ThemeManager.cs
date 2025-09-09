using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Media;

namespace NationalClock.Services;

/// <summary>
/// Service for managing Material Design themes and color schemes
/// Implements Singleton pattern for global access
/// </summary>
public sealed class ThemeManager
{
    private static readonly Lazy<ThemeManager> _instance = new(() => new ThemeManager());
    private readonly PaletteHelper _paletteHelper;
    private bool _isDarkMode = false;
    private string _currentAccentColor = "Blue";

    /// <summary>
    /// Gets the singleton instance of ThemeManager
    /// </summary>
    public static ThemeManager Instance => _instance.Value;

    /// <summary>
    /// Event fired when theme changes
    /// </summary>
    public event EventHandler<bool>? ThemeChanged;

    /// <summary>
    /// Event fired when accent color changes
    /// </summary>
    public event EventHandler<string>? AccentColorChanged;

    /// <summary>
    /// Private constructor to prevent external instantiation
    /// </summary>
    private ThemeManager()
    {
        System.Diagnostics.Debug.WriteLine("ThemeManager: Constructor started");
        _paletteHelper = new PaletteHelper();
        System.Diagnostics.Debug.WriteLine($"ThemeManager: Before InitializeTheme - _isDarkMode: {_isDarkMode}");
        InitializeTheme();
        System.Diagnostics.Debug.WriteLine($"ThemeManager: After InitializeTheme - _isDarkMode: {_isDarkMode}");
    }

    /// <summary>
    /// Gets whether dark mode is currently active
    /// </summary>
    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            System.Diagnostics.Debug.WriteLine($"ThemeManager.IsDarkMode setter: Current: {_isDarkMode}, New: {value}");
            
            if (_isDarkMode != value)
            {
                _isDarkMode = value;
                System.Diagnostics.Debug.WriteLine($"ThemeManager.IsDarkMode setter: Value changed, calling ApplyTheme({value})");
                ApplyTheme(value);
                ThemeChanged?.Invoke(this, value);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"ThemeManager.IsDarkMode setter: Value unchanged, skipping ApplyTheme");
            }
        }
    }

    /// <summary>
    /// Gets or sets the current accent color
    /// </summary>
    public string CurrentAccentColor
    {
        get => _currentAccentColor;
        set
        {
            if (!string.IsNullOrEmpty(value) && _currentAccentColor != value)
            {
                _currentAccentColor = value;
                ApplyAccentColor(value);
                AccentColorChanged?.Invoke(this, value);
            }
        }
    }

    /// <summary>
    /// Gets the available accent colors for Material Design
    /// </summary>
    public static Dictionary<string, Color> AvailableAccentColors => new()
    {
        { "Red", Colors.Red },
        { "Pink", Colors.DeepPink },
        { "Purple", Colors.Purple },
        { "Deep Purple", Colors.BlueViolet },
        { "Indigo", Colors.Indigo },
        { "Blue", Colors.Blue },
        { "Light Blue", Colors.LightBlue },
        { "Cyan", Colors.Cyan },
        { "Teal", Colors.Teal },
        { "Green", Colors.Green },
        { "Light Green", Colors.LightGreen },
        { "Lime", Colors.Lime },
        { "Yellow", Colors.Yellow },
        { "Amber", Colors.Orange },
        { "Orange", Colors.OrangeRed },
        { "Deep Orange", Colors.DarkOrange },
        { "Brown", Colors.Brown },
        { "Blue Grey", Colors.SlateGray }
    };

    /// <summary>
    /// Initializes the theme system with basic Material Design setup
    /// Actual theme will be applied later via ApplySettingsTheme
    /// </summary>
    private void InitializeTheme()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("ThemeManager.InitializeTheme: Starting basic initialization");
            
            // Set initial defaults but don't apply theme yet
            // The actual theme will be applied by ApplySettingsTheme() in App.cs
            _isDarkMode = false;
            _currentAccentColor = "Blue";
            
            System.Diagnostics.Debug.WriteLine($"ThemeManager.InitializeTheme: Set default values - _isDarkMode: {_isDarkMode}, _currentAccentColor: {_currentAccentColor}");
            System.Diagnostics.Debug.WriteLine("ThemeManager.InitializeTheme: Basic initialization completed - theme will be applied by ApplySettingsTheme()");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing theme: {ex.Message}");
        }
    }

    /// <summary>
    /// Applies the specified theme (light or dark)
    /// </summary>
    /// <param name="isDark">True for dark theme, false for light theme</param>
    private void ApplyTheme(bool isDark)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"ThemeManager.ApplyTheme: Applying theme - isDark: {isDark}");
            
            var theme = _paletteHelper.GetTheme();
            
            // Set base theme
            theme.SetBaseTheme(isDark ? BaseTheme.Dark : BaseTheme.Light);
            
            // Set base theme only - MaterialDesign handles background colors automatically
            // The previous background color issue was likely due to incorrect usage of theme properties
            
            _paletteHelper.SetTheme(theme);
            
            System.Diagnostics.Debug.WriteLine($"ThemeManager.ApplyTheme: Theme applied successfully - BaseTheme: {(isDark ? "Dark" : "Light")}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error applying theme: {ex.Message}");
        }
    }

    /// <summary>
    /// Applies the specified accent color
    /// </summary>
    /// <param name="colorName">Name of the accent color</param>
    private void ApplyAccentColor(string colorName)
    {
        try
        {
            if (AvailableAccentColors.TryGetValue(colorName, out Color color))
            {
                var theme = _paletteHelper.GetTheme();
                
                // Set primary color based on color name with Material Design color variants
                switch (colorName.ToLower())
                {
                    case "red":
                        theme.SetPrimaryColor(Color.FromRgb(244, 67, 54));  // Material Red 500
                        theme.SetSecondaryColor(Color.FromRgb(255, 138, 128)); // Red A100
                        break;
                    case "pink":
                        theme.SetPrimaryColor(Color.FromRgb(233, 30, 99));  // Material Pink 500
                        theme.SetSecondaryColor(Color.FromRgb(255, 128, 171)); // Pink A100
                        break;
                    case "purple":
                        theme.SetPrimaryColor(Color.FromRgb(156, 39, 176)); // Material Purple 500
                        theme.SetSecondaryColor(Color.FromRgb(234, 128, 252)); // Purple A100
                        break;
                    case "deep purple":
                        theme.SetPrimaryColor(Color.FromRgb(103, 58, 183)); // Material Deep Purple 500
                        theme.SetSecondaryColor(Color.FromRgb(179, 136, 255)); // Deep Purple A100
                        break;
                    case "indigo":
                        theme.SetPrimaryColor(Color.FromRgb(63, 81, 181));  // Material Indigo 500
                        theme.SetSecondaryColor(Color.FromRgb(140, 158, 255)); // Indigo A100
                        break;
                    case "blue":
                        theme.SetPrimaryColor(Color.FromRgb(33, 150, 243)); // Material Blue 500
                        theme.SetSecondaryColor(Color.FromRgb(130, 177, 255)); // Blue A100
                        break;
                    case "light blue":
                        theme.SetPrimaryColor(Color.FromRgb(3, 169, 244));  // Material Light Blue 500
                        theme.SetSecondaryColor(Color.FromRgb(128, 216, 255)); // Light Blue A100
                        break;
                    case "cyan":
                        theme.SetPrimaryColor(Color.FromRgb(0, 188, 212));  // Material Cyan 500
                        theme.SetSecondaryColor(Color.FromRgb(132, 255, 255)); // Cyan A100
                        break;
                    case "teal":
                        theme.SetPrimaryColor(Color.FromRgb(0, 150, 136));  // Material Teal 500
                        theme.SetSecondaryColor(Color.FromRgb(167, 255, 235)); // Teal A100
                        break;
                    case "green":
                        theme.SetPrimaryColor(Color.FromRgb(76, 175, 80));  // Material Green 500
                        theme.SetSecondaryColor(Color.FromRgb(185, 246, 202)); // Green A100
                        break;
                    case "light green":
                        theme.SetPrimaryColor(Color.FromRgb(139, 195, 74)); // Material Light Green 500
                        theme.SetSecondaryColor(Color.FromRgb(204, 255, 144)); // Light Green A100
                        break;
                    case "lime":
                        theme.SetPrimaryColor(Color.FromRgb(205, 220, 57)); // Material Lime 500
                        theme.SetSecondaryColor(Color.FromRgb(240, 244, 195)); // Lime A100
                        break;
                    case "yellow":
                        theme.SetPrimaryColor(Color.FromRgb(255, 235, 59)); // Material Yellow 500
                        theme.SetSecondaryColor(Color.FromRgb(255, 255, 141)); // Yellow A100
                        break;
                    case "amber":
                        theme.SetPrimaryColor(Color.FromRgb(255, 193, 7));  // Material Amber 500
                        theme.SetSecondaryColor(Color.FromRgb(255, 229, 127)); // Amber A100
                        break;
                    case "orange":
                        theme.SetPrimaryColor(Color.FromRgb(255, 152, 0));  // Material Orange 500
                        theme.SetSecondaryColor(Color.FromRgb(255, 209, 128)); // Orange A100
                        break;
                    case "deep orange":
                        theme.SetPrimaryColor(Color.FromRgb(255, 87, 34));  // Material Deep Orange 500
                        theme.SetSecondaryColor(Color.FromRgb(255, 158, 128)); // Deep Orange A100
                        break;
                    case "brown":
                        theme.SetPrimaryColor(Color.FromRgb(121, 85, 72));  // Material Brown 500
                        theme.SetSecondaryColor(Color.FromRgb(215, 204, 200)); // Brown 100
                        break;
                    case "blue grey":
                        theme.SetPrimaryColor(Color.FromRgb(96, 125, 139)); // Material Blue Grey 500
                        theme.SetSecondaryColor(Color.FromRgb(207, 216, 220)); // Blue Grey 100
                        break;
                    default:
                        theme.SetPrimaryColor(Color.FromRgb(33, 150, 243)); // Default to Material Blue 500
                        theme.SetSecondaryColor(Color.FromRgb(130, 177, 255)); // Blue A100
                        break;
                }
                
                _paletteHelper.SetTheme(theme);
                
                System.Diagnostics.Debug.WriteLine($"ThemeManager.ApplyAccentColor: Applied {colorName} color successfully");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error applying accent color {colorName}: {ex.Message}");
        }
    }

    /// <summary>
    /// Toggles between light and dark theme
    /// </summary>
    public void ToggleTheme()
    {
        IsDarkMode = !IsDarkMode;
    }

    /// <summary>
    /// Sets the theme based on system preference (if available)
    /// </summary>
    public void SetSystemTheme()
    {
        try
        {
            // Try to get system theme preference from registry
            var registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            if (registryKey != null)
            {
                var appsUseLightTheme = registryKey.GetValue("AppsUseLightTheme");
                if (appsUseLightTheme is int lightTheme)
                {
                    IsDarkMode = lightTheme == 0; // 0 = dark theme, 1 = light theme
                }
                registryKey.Close();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error reading system theme: {ex.Message}");
            // Fallback to light theme
            IsDarkMode = false;
        }
    }

    /// <summary>
    /// Applies theme settings from the Settings model
    /// </summary>
    /// <param name="settings">Settings containing theme preferences</param>
    public void ApplySettingsTheme(Models.Settings settings)
    {
        if (settings == null) return;

        System.Diagnostics.Debug.WriteLine($"ThemeManager.ApplySettingsTheme: Applying IsDarkMode: {settings.IsDarkMode}, AccentColor: {settings.ThemeAccentColor}");
        
        // Update internal fields first to avoid triggering events during initialization
        _isDarkMode = settings.IsDarkMode;
        _currentAccentColor = settings.ThemeAccentColor;
        
        // Apply the theme
        ApplyTheme(settings.IsDarkMode);
        ApplyAccentColor(settings.ThemeAccentColor);
        
        System.Diagnostics.Debug.WriteLine($"ThemeManager.ApplySettingsTheme: Applied theme successfully");
    }

    /// <summary>
    /// Gets the current theme as a Theme object
    /// </summary>
    /// <returns>Current Material Design theme</returns>
    public Theme GetCurrentTheme()
    {
        return _paletteHelper.GetTheme();
    }

    /// <summary>
    /// Gets the current primary color
    /// </summary>
    /// <returns>Primary color of current theme</returns>
    public Color GetPrimaryColor()
    {
        try
        {
            var theme = _paletteHelper.GetTheme();
            return theme.PrimaryMid.Color;
        }
        catch
        {
            return Colors.Blue;
        }
    }

    /// <summary>
    /// Gets the current secondary color
    /// </summary>
    /// <returns>Secondary color of current theme</returns>
    public Color GetSecondaryColor()
    {
        try
        {
            var theme = _paletteHelper.GetTheme();
            return theme.SecondaryMid.Color;
        }
        catch
        {
            return Colors.LightBlue;
        }
    }

    /// <summary>
    /// Gets the current background color
    /// </summary>
    /// <returns>Background color of current theme</returns>
    public Color GetBackgroundColor()
    {
        try
        {
            var theme = _paletteHelper.GetTheme();
            return theme.Background;
        }
        catch
        {
            return IsDarkMode ? Color.FromRgb(18, 18, 18) : Colors.White;
        }
    }

    /// <summary>
    /// Gets the current text color
    /// </summary>
    /// <returns>Text color of current theme</returns>
    public Color GetTextColor()
    {
        try
        {
            var theme = _paletteHelper.GetTheme();
            return theme.Foreground;
        }
        catch
        {
            return IsDarkMode ? Colors.White : Color.FromRgb(33, 33, 33);
        }
    }

    /// <summary>
    /// Creates a SolidColorBrush from the current theme's primary color
    /// </summary>
    /// <returns>SolidColorBrush with primary color</returns>
    public SolidColorBrush GetPrimaryBrush()
    {
        return new SolidColorBrush(GetPrimaryColor());
    }

    /// <summary>
    /// Creates a SolidColorBrush from the current theme's secondary color
    /// </summary>
    /// <returns>SolidColorBrush with secondary color</returns>
    public SolidColorBrush GetSecondaryBrush()
    {
        return new SolidColorBrush(GetSecondaryColor());
    }

    /// <summary>
    /// Creates a SolidColorBrush from the current theme's background color
    /// </summary>
    /// <returns>SolidColorBrush with background color</returns>
    public SolidColorBrush GetBackgroundBrush()
    {
        return new SolidColorBrush(GetBackgroundColor());
    }

    /// <summary>
    /// Creates a SolidColorBrush from the current theme's text color
    /// </summary>
    /// <returns>SolidColorBrush with text color</returns>
    public SolidColorBrush GetTextBrush()
    {
        return new SolidColorBrush(GetTextColor());
    }

    /// <summary>
    /// Validates if a color name is available as an accent color
    /// </summary>
    /// <param name="colorName">Name of the color to validate</param>
    /// <returns>True if the color is available</returns>
    public static bool IsValidAccentColor(string colorName)
    {
        return !string.IsNullOrEmpty(colorName) && AvailableAccentColors.ContainsKey(colorName);
    }

    /// <summary>
    /// Gets a color by name from the available accent colors
    /// </summary>
    /// <param name="colorName">Name of the color</param>
    /// <returns>Color value or default blue if not found</returns>
    public static Color GetColorByName(string colorName)
    {
        return AvailableAccentColors.TryGetValue(colorName, out Color color) ? color : Colors.Blue;
    }

    /// <summary>
    /// Refreshes the current theme (useful after system theme changes)
    /// </summary>
    public void RefreshTheme()
    {
        ApplyTheme(_isDarkMode);
        ApplyAccentColor(_currentAccentColor);
    }

    /// <summary>
    /// Gets the Material Design color for the specified color name
    /// </summary>
    /// <param name="colorName">Name of the color</param>
    /// <returns>Material Design color if found, otherwise null</returns>
    public static Color? GetMaterialDesignColor(string colorName)
    {
        if (string.IsNullOrEmpty(colorName))
            return null;
            
        return AvailableAccentColors.TryGetValue(colorName, out Color color) ? color : null;
    }

    /// <summary>
    /// Gets theme information as a string
    /// </summary>
    /// <returns>String representation of current theme</returns>
    public override string ToString()
    {
        return $"Theme: {(IsDarkMode ? "Dark" : "Light")}, Accent: {CurrentAccentColor}";
    }
}
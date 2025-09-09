using NationalClock.Models;
using System.IO;
using System.Text.Json;

namespace NationalClock.Services;

/// <summary>
/// Service for managing application settings with JSON file persistence
/// Implements Singleton pattern for global access
/// </summary>
public sealed class SettingsManager
{
    private static readonly Lazy<SettingsManager> _instance = new(() => new SettingsManager());
    private readonly string _settingsFilePath;
    private readonly object _lock = new();
    private Settings _currentSettings;

    /// <summary>
    /// Gets the singleton instance of SettingsManager
    /// </summary>
    public static SettingsManager Instance => _instance.Value;

    /// <summary>
    /// Event fired when settings are changed and saved
    /// </summary>
    public event EventHandler<Settings>? SettingsChanged;

    /// <summary>
    /// Private constructor to prevent external instantiation
    /// </summary>
    private SettingsManager()
    {
        // Set settings file path in AppData/Local
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "NationalClock");
        
        // Ensure directory exists
        Directory.CreateDirectory(appFolder);
        
        _settingsFilePath = Path.Combine(appFolder, "settings.json");
        
        // Load settings or create default
        _currentSettings = LoadSettings();
    }

    /// <summary>
    /// Gets the current settings
    /// </summary>
    public Settings CurrentSettings
    {
        get
        {
            lock (_lock)
            {
                return _currentSettings.Clone(); // Return a copy to prevent external modifications
            }
        }
    }

    /// <summary>
    /// Gets the settings file path
    /// </summary>
    public string SettingsFilePath => _settingsFilePath;

    /// <summary>
    /// Loads settings from the JSON file or creates default settings
    /// </summary>
    /// <returns>Settings object</returns>
    private Settings LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                var json = File.ReadAllText(_settingsFilePath);
                var settings = JsonSerializer.Deserialize<Settings>(json, GetJsonOptions());
                
                if (settings != null)
                {
                    // Validate and fix any invalid settings
                    settings.ValidateAndFix();
                    return settings;
                }
            }
        }
        catch (Exception ex)
        {
            // Log error (for now just create default settings)
            System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
        }

        // Return default settings if loading failed or file doesn't exist
        var defaultSettings = new Settings();
        defaultSettings.ValidateAndFix();
        
        // Save default settings
        try
        {
            SaveSettings(defaultSettings);
        }
        catch
        {
            // Ignore save errors during initialization
        }
        
        return defaultSettings;
    }

    /// <summary>
    /// Saves the current settings to the JSON file
    /// </summary>
    /// <returns>True if saved successfully, false otherwise</returns>
    public bool SaveSettings()
    {
        lock (_lock)
        {
            return SaveSettings(_currentSettings);
        }
    }

    /// <summary>
    /// Saves the specified settings to the JSON file
    /// </summary>
    /// <param name="settings">The settings to save</param>
    /// <returns>True if saved successfully, false otherwise</returns>
    public bool SaveSettings(Settings settings)
    {
        if (settings == null) return false;

        try
        {
            // Validate settings before saving
            settings.ValidateAndFix();
            
            // Serialize to JSON
            var json = JsonSerializer.Serialize(settings, GetJsonOptions());
            
            // Write to file with backup
            var backupPath = _settingsFilePath + ".backup";
            
            // Create backup of existing file
            if (File.Exists(_settingsFilePath))
            {
                File.Copy(_settingsFilePath, backupPath, true);
            }
            
            // Write new settings
            File.WriteAllText(_settingsFilePath, json);
            
            // Update current settings and notify
            lock (_lock)
            {
                _currentSettings = settings.Clone();
            }
            
            SettingsChanged?.Invoke(this, settings);
            
            // Remove backup on successful save
            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }
            
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
            
            // Try to restore from backup
            try
            {
                var backupPath = _settingsFilePath + ".backup";
                if (File.Exists(backupPath))
                {
                    File.Copy(backupPath, _settingsFilePath, true);
                }
            }
            catch
            {
                // Backup restore failed, ignore
            }
            
            return false;
        }
    }

    /// <summary>
    /// Updates a specific setting and saves immediately
    /// </summary>
    /// <param name="updateAction">Action to update the settings</param>
    /// <returns>True if updated and saved successfully</returns>
    public bool UpdateSetting(Action<Settings> updateAction)
    {
        if (updateAction == null) return false;

        lock (_lock)
        {
            var newSettings = _currentSettings.Clone();
            updateAction(newSettings);
            return SaveSettings(newSettings);
        }
    }

    /// <summary>
    /// Updates multiple settings in a batch and saves once
    /// </summary>
    /// <param name="updates">Dictionary of setting updates</param>
    /// <returns>True if all updates applied and saved successfully</returns>
    public bool UpdateSettings(Dictionary<string, object> updates)
    {
        if (updates == null || updates.Count == 0) return false;

        lock (_lock)
        {
            var newSettings = _currentSettings.Clone();
            
            foreach (var update in updates)
            {
                try
                {
                    var property = typeof(Settings).GetProperty(update.Key);
                    if (property != null && property.CanWrite)
                    {
                        // Convert value to proper type if needed
                        var value = Convert.ChangeType(update.Value, property.PropertyType);
                        property.SetValue(newSettings, value);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error updating setting {update.Key}: {ex.Message}");
                }
            }
            
            return SaveSettings(newSettings);
        }
    }

    /// <summary>
    /// Gets a specific setting value
    /// </summary>
    /// <typeparam name="T">The type of the setting value</typeparam>
    /// <param name="propertyName">The property name</param>
    /// <returns>The setting value or default value if not found</returns>
    public T GetSetting<T>(string propertyName)
    {
        lock (_lock)
        {
            try
            {
                var property = typeof(Settings).GetProperty(propertyName);
                if (property != null && property.CanRead)
                {
                    var value = property.GetValue(_currentSettings);
                    if (value is T typedValue)
                        return typedValue;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting setting {propertyName}: {ex.Message}");
            }
            
            return default(T)!;
        }
    }

    /// <summary>
    /// Sets a specific setting value
    /// </summary>
    /// <typeparam name="T">The type of the setting value</typeparam>
    /// <param name="propertyName">The property name</param>
    /// <param name="value">The value to set</param>
    /// <returns>True if set and saved successfully</returns>
    public bool SetSetting<T>(string propertyName, T value)
    {
        return UpdateSetting(settings =>
        {
            try
            {
                var property = typeof(Settings).GetProperty(propertyName);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(settings, value);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting {propertyName}: {ex.Message}");
            }
        });
    }

    /// <summary>
    /// Resets settings to default values
    /// </summary>
    /// <returns>True if reset and saved successfully</returns>
    public bool ResetToDefaults()
    {
        var defaultSettings = new Settings();
        defaultSettings.ValidateAndFix();
        return SaveSettings(defaultSettings);
    }

    /// <summary>
    /// Exports settings to a specified file path
    /// </summary>
    /// <param name="exportPath">The path to export to</param>
    /// <returns>True if exported successfully</returns>
    public bool ExportSettings(string exportPath)
    {
        if (string.IsNullOrWhiteSpace(exportPath)) return false;

        try
        {
            lock (_lock)
            {
                var json = JsonSerializer.Serialize(_currentSettings, GetJsonOptions());
                File.WriteAllText(exportPath, json);
                return true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error exporting settings: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Imports settings from a specified file path
    /// </summary>
    /// <param name="importPath">The path to import from</param>
    /// <returns>True if imported and saved successfully</returns>
    public bool ImportSettings(string importPath)
    {
        if (string.IsNullOrWhiteSpace(importPath) || !File.Exists(importPath)) return false;

        try
        {
            var json = File.ReadAllText(importPath);
            var settings = JsonSerializer.Deserialize<Settings>(json, GetJsonOptions());
            
            if (settings != null)
            {
                settings.ValidateAndFix();
                return SaveSettings(settings);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error importing settings: {ex.Message}");
        }

        return false;
    }

    /// <summary>
    /// Gets the JSON serialization options
    /// </summary>
    /// <returns>JsonSerializerOptions configured for settings serialization</returns>
    private static JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            Converters = 
            {
                // Add custom converters if needed
            }
        };
    }

    /// <summary>
    /// Gets information about the settings file
    /// </summary>
    /// <returns>FileInfo about the settings file or null if it doesn't exist</returns>
    public FileInfo? GetSettingsFileInfo()
    {
        try
        {
            return File.Exists(_settingsFilePath) ? new FileInfo(_settingsFilePath) : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Checks if the settings file exists
    /// </summary>
    /// <returns>True if the settings file exists</returns>
    public bool SettingsFileExists()
    {
        return File.Exists(_settingsFilePath);
    }

    /// <summary>
    /// Creates a backup of the current settings file
    /// </summary>
    /// <param name="backupPath">Optional backup path (if null, uses default backup location)</param>
    /// <returns>True if backup created successfully</returns>
    public bool BackupSettings(string? backupPath = null)
    {
        try
        {
            if (!File.Exists(_settingsFilePath)) return false;

            backupPath ??= _settingsFilePath + $".backup.{DateTime.Now:yyyyMMdd_HHmmss}";
            File.Copy(_settingsFilePath, backupPath, true);
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error backing up settings: {ex.Message}");
            return false;
        }
    }
}
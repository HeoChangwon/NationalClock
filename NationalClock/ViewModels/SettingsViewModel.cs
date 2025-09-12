using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NationalClock.Models;
using NationalClock.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace NationalClock.ViewModels;

/// <summary>
/// ViewModel for the Settings window
/// Manages timezone selection, theme options, and application preferences
/// </summary>
public partial class SettingsViewModel : BaseViewModel
{
    private readonly TimeZoneManager _timeZoneManager;
    private readonly ThemeManager _themeManager;
    private readonly SettingsManager _settingsManager;
    private readonly ClockService _clockService;
    private Settings _originalSettings;
    private Settings _workingSettings;

    [ObservableProperty]
    private ObservableCollection<Models.TimeZoneInfo> _availableTimeZones;

    [ObservableProperty]
    private ObservableCollection<Models.TimeZoneInfo> _enabledTimeZones;

    [ObservableProperty]
    private Models.TimeZoneInfo? _selectedAvailableTimeZone;

    [ObservableProperty]
    private Models.TimeZoneInfo? _selectedEnabledTimeZone;

    [ObservableProperty]
    private bool _is24HourFormat = true;

    [ObservableProperty]
    private bool _isDarkMode = false;

    [ObservableProperty]
    private bool _isAlwaysOnTop = false;

    [ObservableProperty]
    private bool _showSeconds = true;

    [ObservableProperty]
    private bool _showDate = true;

    [ObservableProperty]
    private bool _showTimeZoneOffset = true;

    [ObservableProperty]
    private bool _isCompactMode = false;

    [ObservableProperty]
    private bool _autoStartWithWindows = false;

    [ObservableProperty]
    private string _selectedAccentColor = "Blue";

    [ObservableProperty]
    private string _selectedBackgroundColor = "Default";

    [ObservableProperty]
    private int _updateIntervalSeconds = 1;

    [ObservableProperty]
    private bool _hasUnsavedChanges = false;

    /// <summary>
    /// Constructor with dependency injection of required services
    /// </summary>
    /// <param name="timeZoneManager">Service for managing timezone data</param>
    /// <param name="themeManager">Service for managing Material Design themes</param>
    /// <param name="settingsManager">Service for managing application settings</param>
    /// <param name="clockService">Service for managing clock operations</param>
    public SettingsViewModel(
        TimeZoneManager timeZoneManager,
        ThemeManager themeManager,
        SettingsManager settingsManager,
        ClockService clockService)
    {
        _timeZoneManager = timeZoneManager ?? TimeZoneManager.Instance;
        _themeManager = themeManager ?? ThemeManager.Instance;
        _settingsManager = settingsManager ?? SettingsManager.Instance;
        _clockService = clockService ?? ClockService.Instance;

        _availableTimeZones = new ObservableCollection<Models.TimeZoneInfo>();
        _enabledTimeZones = new ObservableCollection<Models.TimeZoneInfo>();

        // Store original settings for cancel functionality
        _originalSettings = _settingsManager.CurrentSettings;
        _workingSettings = _originalSettings.Clone();

        InitializeFromSettings();
        LoadTimeZones();

        // Subscribe to property changes to track unsaved changes
        PropertyChanged += OnPropertyChanged;
    }

    /// <summary>
    /// Gets the available accent colors for Material Design themes
    /// </summary>
    public Dictionary<string, System.Windows.Media.Color> AvailableAccentColors => 
        ThemeManager.AvailableAccentColors;

    /// <summary>
    /// Gets the accent color names for binding to UI
    /// </summary>
    public List<string> AccentColorNames => AvailableAccentColors.Keys.ToList();

    /// <summary>
    /// Gets the available background colors for UI binding
    /// </summary>
    public static Dictionary<string, System.Windows.Media.Color> AvailableBackgroundColors => new()
    {
        { "Default", System.Windows.Media.Colors.Transparent },
        { "White", System.Windows.Media.Colors.White },
        { "LightBlue", System.Windows.Media.Colors.LightBlue },
        { "LightGray", System.Windows.Media.Colors.LightGray },
        { "LightGreen", System.Windows.Media.Colors.LightGreen },
        { "LightYellow", System.Windows.Media.Colors.LightYellow },
        { "LightPink", System.Windows.Media.Colors.LightPink },
        { "LightCyan", System.Windows.Media.Colors.LightCyan },
        { "Beige", System.Windows.Media.Colors.Beige },
        { "Lavender", System.Windows.Media.Colors.Lavender }
    };

    /// <summary>
    /// Gets the background color names for binding to UI
    /// </summary>
    public List<string> BackgroundColorNames => AvailableBackgroundColors.Keys.ToList();

    /// <summary>
    /// Command to preview the selected background color
    /// </summary>
    [RelayCommand]
    private void PreviewBackground()
    {
        try
        {
            if (string.IsNullOrEmpty(SelectedBackgroundColor))
                return;

            System.Diagnostics.Debug.WriteLine($"Previewing background color: {SelectedBackgroundColor}");

            // Find MainWindow and apply preview background color
            var mainWindow = System.Windows.Application.Current.MainWindow;
            if (mainWindow is MainWindow window)
            {
                // Apply custom background color temporarily
                if (SelectedBackgroundColor == "Default")
                {
                    // Restore to theme-based background
                    var settings = _settingsManager.CurrentSettings;
                    if (settings.IsDarkMode)
                    {
                        window.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(48, 48, 48));
                    }
                    else
                    {
                        window.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                    }
                }
                else
                {
                    try
                    {
                        var colorProperty = typeof(System.Windows.Media.Colors).GetProperty(SelectedBackgroundColor);
                        if (colorProperty != null)
                        {
                            var color = (System.Windows.Media.Color)colorProperty.GetValue(null)!;
                            window.Background = new System.Windows.Media.SolidColorBrush(color);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error applying preview color: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error previewing background color: {ex.Message}");
        }
    }

    /// <summary>
    /// Restores the original background color (used when canceling changes)
    /// </summary>
    private void RestoreOriginalBackgroundColor()
    {
        try
        {
            var mainWindow = System.Windows.Application.Current.MainWindow;
            if (mainWindow is MainWindow window)
            {
                var originalColor = _originalSettings.BackgroundColor;
                
                if (originalColor == "Default")
                {
                    // Restore to theme-based background
                    if (_originalSettings.IsDarkMode)
                    {
                        window.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(48, 48, 48));
                    }
                    else
                    {
                        window.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                    }
                }
                else
                {
                    try
                    {
                        var colorProperty = typeof(System.Windows.Media.Colors).GetProperty(originalColor);
                        if (colorProperty != null)
                        {
                            var color = (System.Windows.Media.Color)colorProperty.GetValue(null)!;
                            window.Background = new System.Windows.Media.SolidColorBrush(color);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error restoring original background color: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error restoring original background color: {ex.Message}");
        }
    }

    /// <summary>
    /// Command to add the selected timezone to enabled list
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAddTimeZone))]
    private void AddTimeZone()
    {
        if (SelectedAvailableTimeZone != null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Adding timezone: {SelectedAvailableTimeZone.DisplayName} (ID: {SelectedAvailableTimeZone.Id})");
                
                // Ensure collections are not null
                if (AvailableTimeZones == null)
                {
                    System.Diagnostics.Debug.WriteLine("AvailableTimeZones is null!");
                    return;
                }
                
                if (EnabledTimeZones == null)
                {
                    System.Diagnostics.Debug.WriteLine("EnabledTimeZones is null!");
                    return;
                }
                
                if (_workingSettings == null)
                {
                    System.Diagnostics.Debug.WriteLine("_workingSettings is null!");
                    return;
                }

                var timeZoneToMove = SelectedAvailableTimeZone;

                // Ensure UI update happens on UI thread
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    // Move from available to enabled
                    AvailableTimeZones.Remove(timeZoneToMove);
                    EnabledTimeZones.Add(timeZoneToMove);
                });

                // Update working settings - ensure EnabledTimeZoneIds is not null
                if (_workingSettings.EnabledTimeZoneIds == null)
                {
                    _workingSettings.EnabledTimeZoneIds = new List<int>();
                }
                
                _workingSettings.EnabledTimeZoneIds = EnabledTimeZones
                    .Where(tz => tz != null)
                    .Select(tz => tz.Id)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"Updated EnabledTimeZoneIds: [{string.Join(", ", _workingSettings.EnabledTimeZoneIds)}]");
                System.Diagnostics.Debug.WriteLine($"AvailableTimeZones count: {AvailableTimeZones.Count}");
                System.Diagnostics.Debug.WriteLine($"EnabledTimeZones count: {EnabledTimeZones.Count}");

                // Clear selection
                SelectedAvailableTimeZone = null;

                // Mark as changed
                HasUnsavedChanges = true;

                // Update command states
                AddTimeZoneCommand.NotifyCanExecuteChanged();
                RemoveTimeZoneCommand.NotifyCanExecuteChanged();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding timezone: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }

    /// <summary>
    /// Determines if a timezone can be added
    /// </summary>
    private bool CanAddTimeZone()
    {
        return SelectedAvailableTimeZone != null;
    }

    /// <summary>
    /// Command to remove the selected timezone from enabled list
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRemoveTimeZone))]
    private void RemoveTimeZone()
    {
        if (SelectedEnabledTimeZone != null && EnabledTimeZones.Count > 1) // Keep at least one timezone
        {
            try
            {
                // Move from enabled to available
                EnabledTimeZones.Remove(SelectedEnabledTimeZone);
                AvailableTimeZones.Add(SelectedEnabledTimeZone);

                // Sort available timezones by display order (filter out nulls)
                var sortedAvailable = AvailableTimeZones
                    .Where(tz => tz != null)
                    .OrderBy(tz => tz.DisplayOrder)
                    .ToList();
                AvailableTimeZones.Clear();
                foreach (var tz in sortedAvailable)
                {
                    AvailableTimeZones.Add(tz);
                }

                // Update working settings (filter out nulls)
                _workingSettings.EnabledTimeZoneIds = EnabledTimeZones
                    .Where(tz => tz != null)
                    .Select(tz => tz.Id)
                    .ToList();

                // Clear selection
                SelectedEnabledTimeZone = null;

                // Mark as changed
                HasUnsavedChanges = true;

                // Update command states
                AddTimeZoneCommand.NotifyCanExecuteChanged();
                RemoveTimeZoneCommand.NotifyCanExecuteChanged();
                MoveUpCommand.NotifyCanExecuteChanged();
                MoveDownCommand.NotifyCanExecuteChanged();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error removing timezone: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Determines if a timezone can be removed
    /// </summary>
    private bool CanRemoveTimeZone()
    {
        return SelectedEnabledTimeZone != null && EnabledTimeZones.Count > 1;
    }

    /// <summary>
    /// Command to move the selected enabled timezone up in the list
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanMoveUp))]
    private void MoveUp()
    {
        if (SelectedEnabledTimeZone != null)
        {
            try
            {
                var currentIndex = EnabledTimeZones.IndexOf(SelectedEnabledTimeZone);
                if (currentIndex > 0)
                {
                    // Swap positions
                    EnabledTimeZones.Move(currentIndex, currentIndex - 1);

                    // Update display orders and working settings
                    UpdateDisplayOrders();
                    HasUnsavedChanges = true;

                    // Update command states
                    MoveUpCommand.NotifyCanExecuteChanged();
                    MoveDownCommand.NotifyCanExecuteChanged();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error moving timezone up: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Determines if the selected timezone can be moved up
    /// </summary>
    private bool CanMoveUp()
    {
        return SelectedEnabledTimeZone != null && 
               EnabledTimeZones.IndexOf(SelectedEnabledTimeZone) > 0;
    }

    /// <summary>
    /// Command to move the selected enabled timezone down in the list
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanMoveDown))]
    private void MoveDown()
    {
        if (SelectedEnabledTimeZone != null)
        {
            try
            {
                var currentIndex = EnabledTimeZones.IndexOf(SelectedEnabledTimeZone);
                if (currentIndex < EnabledTimeZones.Count - 1)
                {
                    // Swap positions
                    EnabledTimeZones.Move(currentIndex, currentIndex + 1);

                    // Update display orders and working settings
                    UpdateDisplayOrders();
                    HasUnsavedChanges = true;

                    // Update command states
                    MoveUpCommand.NotifyCanExecuteChanged();
                    MoveDownCommand.NotifyCanExecuteChanged();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error moving timezone down: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Determines if the selected timezone can be moved down
    /// </summary>
    private bool CanMoveDown()
    {
        return SelectedEnabledTimeZone != null && 
               EnabledTimeZones.IndexOf(SelectedEnabledTimeZone) < EnabledTimeZones.Count - 1;
    }

    /// <summary>
    /// Command to save all changes and apply settings
    /// </summary>
    [RelayCommand]
    private void Save()
    {
        try
        {
            // Update working settings with current property values
            UpdateWorkingSettings();

            // Save to settings manager
            var success = _settingsManager.SaveSettings(_workingSettings);

            if (success)
            {
                // Update original settings reference
                _originalSettings = _workingSettings.Clone();

                // Apply timezone changes to TimeZoneManager
                ApplyTimeZoneChanges();

                // Apply theme changes using the proper method
                _themeManager.ApplySettingsTheme(_workingSettings);

                // Apply clock service settings
                _clockService.Is24HourFormat = Is24HourFormat;
                _clockService.UpdateIntervalSeconds = UpdateIntervalSeconds;

                // Refresh clocks with new settings
                _clockService.RefreshClocks();

                // Clear unsaved changes flag
                HasUnsavedChanges = false;

                System.Diagnostics.Debug.WriteLine("Settings saved successfully");

                // Close settings window (will be implemented in Phase 4)
                RequestClose?.Invoke();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Failed to save settings");
                // Show error message (will be implemented in Phase 4)
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
        }
    }

    /// <summary>
    /// Command to cancel changes and restore original settings
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        try
        {
            // Restore original settings
            _workingSettings = _originalSettings.Clone();
            InitializeFromSettings();
            LoadTimeZones();

            // Clear unsaved changes flag
            HasUnsavedChanges = false;

            // Restore original background color
            RestoreOriginalBackgroundColor();

            System.Diagnostics.Debug.WriteLine("Settings changes cancelled");

            // Close settings window (will be implemented in Phase 4)
            RequestClose?.Invoke();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error cancelling settings: {ex.Message}");
        }
    }

    /// <summary>
    /// Command to reset all settings to defaults
    /// </summary>
    [RelayCommand]
    private void ResetToDefaults()
    {
        try
        {
            // Show confirmation dialog (will be implemented in Phase 4)
            var result = MessageBox.Show(
                "Are you sure you want to reset all settings to their default values? This action cannot be undone.",
                "Reset to Defaults",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Create default settings
                var defaultSettings = new Settings();
                defaultSettings.ValidateAndFix();

                // Save default settings
                var success = _settingsManager.SaveSettings(defaultSettings);

                if (success)
                {
                    // Update references
                    _originalSettings = defaultSettings;
                    _workingSettings = defaultSettings.Clone();

                    // Reinitialize UI
                    InitializeFromSettings();
                    LoadTimeZones();

                    // Apply changes immediately
                    ApplyTimeZoneChanges();
                    _themeManager.ApplySettingsTheme(_workingSettings);
                    _clockService.Is24HourFormat = Is24HourFormat;
                    _clockService.UpdateIntervalSeconds = UpdateIntervalSeconds;
                    _clockService.RefreshClocks();

                    HasUnsavedChanges = false;

                    System.Diagnostics.Debug.WriteLine("Settings reset to defaults successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Failed to reset settings");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error resetting to defaults: {ex.Message}");
        }
    }

    /// <summary>
    /// Command to preview theme changes without saving
    /// </summary>
    [RelayCommand]
    private void PreviewTheme()
    {
        try
        {
            // Apply theme temporarily for preview
            var previewSettings = _workingSettings.Clone();
            previewSettings.IsDarkMode = IsDarkMode;
            previewSettings.ThemeAccentColor = SelectedAccentColor;
            _themeManager.ApplySettingsTheme(previewSettings);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error previewing theme: {ex.Message}");
        }
    }

    /// <summary>
    /// Event that requests the settings window to close
    /// </summary>
    public event Action? RequestClose;

    /// <summary>
    /// Initializes ViewModel properties from current settings
    /// </summary>
    private void InitializeFromSettings()
    {
        try
        {
            Is24HourFormat = _workingSettings.Is24HourFormat;
            IsDarkMode = _workingSettings.IsDarkMode;
            IsAlwaysOnTop = _workingSettings.IsAlwaysOnTop;
            ShowSeconds = _workingSettings.ShowSeconds;
            ShowDate = _workingSettings.ShowDate;
            ShowTimeZoneOffset = _workingSettings.ShowTimeZoneOffset;
            IsCompactMode = _workingSettings.IsCompactMode;
            AutoStartWithWindows = _workingSettings.AutoStartWithWindows;
            SelectedAccentColor = _workingSettings.ThemeAccentColor;
            SelectedBackgroundColor = _workingSettings.BackgroundColor;
            UpdateIntervalSeconds = _workingSettings.UpdateIntervalSeconds;

            HasUnsavedChanges = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing from settings: {ex.Message}");
        }
    }

    /// <summary>
    /// Loads timezone data into available and enabled collections
    /// </summary>
    private void LoadTimeZones()
    {
        try
        {
            AvailableTimeZones.Clear();
            EnabledTimeZones.Clear();

            var allTimeZones = _timeZoneManager.AllTimeZones;
            var enabledIds = new HashSet<int>(_workingSettings.EnabledTimeZoneIds);

            // Separate into enabled and available
            var enabledList = new List<Models.TimeZoneInfo>();
            var availableList = new List<Models.TimeZoneInfo>();

            foreach (var timeZone in allTimeZones)
            {
                if (enabledIds.Contains(timeZone.Id))
                {
                    enabledList.Add(timeZone);
                }
                else
                {
                    availableList.Add(timeZone);
                }
            }

            // Sort enabled timezones by the order in settings
            var orderedEnabled = enabledList
                .OrderBy(tz => _workingSettings.EnabledTimeZoneIds.IndexOf(tz.Id))
                .ToList();

            // Sort available timezones by display order
            var orderedAvailable = availableList
                .OrderBy(tz => tz.DisplayOrder)
                .ToList();

            // Populate collections
            foreach (var timeZone in orderedEnabled)
            {
                EnabledTimeZones.Add(timeZone);
            }

            foreach (var timeZone in orderedAvailable)
            {
                AvailableTimeZones.Add(timeZone);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading timezones: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates display orders based on current EnabledTimeZones collection order
    /// </summary>
    private void UpdateDisplayOrders()
    {
        try
        {
            if (EnabledTimeZones == null)
            {
                System.Diagnostics.Debug.WriteLine("EnabledTimeZones is null in UpdateDisplayOrders");
                return;
            }

            if (_workingSettings == null)
            {
                System.Diagnostics.Debug.WriteLine("_workingSettings is null in UpdateDisplayOrders");
                return;
            }

            for (int i = 0; i < EnabledTimeZones.Count; i++)
            {
                if (EnabledTimeZones[i] == null)
                {
                    System.Diagnostics.Debug.WriteLine($"EnabledTimeZones[{i}] is null");
                    continue;
                }
                EnabledTimeZones[i].DisplayOrder = i + 1;
            }

            // Update working settings - ensure EnabledTimeZoneIds is not null
            if (_workingSettings.EnabledTimeZoneIds == null)
            {
                _workingSettings.EnabledTimeZoneIds = new List<int>();
            }

            _workingSettings.EnabledTimeZoneIds = EnabledTimeZones
                .Where(tz => tz != null)
                .Select(tz => tz.Id)
                .ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating display orders: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Updates working settings with current property values
    /// </summary>
    private void UpdateWorkingSettings()
    {
        _workingSettings.Is24HourFormat = Is24HourFormat;
        _workingSettings.IsDarkMode = IsDarkMode;
        _workingSettings.IsAlwaysOnTop = IsAlwaysOnTop;
        _workingSettings.ShowSeconds = ShowSeconds;
        _workingSettings.ShowDate = ShowDate;
        _workingSettings.ShowTimeZoneOffset = ShowTimeZoneOffset;
        _workingSettings.IsCompactMode = IsCompactMode;
        _workingSettings.AutoStartWithWindows = AutoStartWithWindows;
        _workingSettings.ThemeAccentColor = SelectedAccentColor;
        _workingSettings.BackgroundColor = SelectedBackgroundColor;
        _workingSettings.UpdateIntervalSeconds = UpdateIntervalSeconds;
        _workingSettings.EnabledTimeZoneIds = EnabledTimeZones
            .Where(tz => tz != null)
            .Select(tz => tz.Id)
            .ToList();
    }

    /// <summary>
    /// Applies timezone changes to the TimeZoneManager
    /// </summary>
    private void ApplyTimeZoneChanges()
    {
        try
        {
            var enabledIds = EnabledTimeZones
                .Where(tz => tz != null)
                .Select(tz => tz.Id)
                .ToList();
            _timeZoneManager.UpdateEnabledTimeZones(enabledIds);

            // Update display orders
            var displayOrders = new Dictionary<int, int>();
            for (int i = 0; i < EnabledTimeZones.Count; i++)
            {
                displayOrders[EnabledTimeZones[i].Id] = i + 1;
            }
            _timeZoneManager.UpdateDisplayOrders(displayOrders);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error applying timezone changes: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles property changes to track unsaved changes
    /// </summary>
    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Update command states when selection changes
        if (e.PropertyName == nameof(SelectedAvailableTimeZone))
        {
            AddTimeZoneCommand.NotifyCanExecuteChanged();
        }
        else if (e.PropertyName == nameof(SelectedEnabledTimeZone))
        {
            RemoveTimeZoneCommand.NotifyCanExecuteChanged();
            MoveUpCommand.NotifyCanExecuteChanged();
            MoveDownCommand.NotifyCanExecuteChanged();
        }
        
        // Track unsaved changes for other properties
        if (e.PropertyName != nameof(HasUnsavedChanges) && 
            e.PropertyName != nameof(SelectedAvailableTimeZone) && 
            e.PropertyName != nameof(SelectedEnabledTimeZone))
        {
            HasUnsavedChanges = true;
        }
    }

    /// <summary>
    /// Checks if there are unsaved changes when attempting to close
    /// </summary>
    /// <returns>True if it's safe to close, false if changes need to be saved</returns>
    public bool CanClose()
    {
        if (!HasUnsavedChanges)
            return true;

        // Show confirmation dialog (will be implemented in Phase 4)
        var result = MessageBox.Show(
            "You have unsaved changes. Do you want to save them before closing?",
            "Unsaved Changes",
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Question);

        switch (result)
        {
            case MessageBoxResult.Yes:
                Save();
                return !HasUnsavedChanges; // Only close if save was successful
            case MessageBoxResult.No:
                return true; // Discard changes and close
            case MessageBoxResult.Cancel:
            default:
                return false; // Don't close
        }
    }
}
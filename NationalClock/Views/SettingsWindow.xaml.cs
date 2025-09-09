using System.ComponentModel;
using System.Windows;
using NationalClock.ViewModels;
using NationalClock.Services;
using MaterialDesignThemes.Wpf;

namespace NationalClock.Views;

/// <summary>
/// Interaction logic for SettingsWindow.xaml
/// </summary>
public partial class SettingsWindow : Window
{
    private readonly SettingsViewModel _viewModel;
    // private readonly ISnackbarMessageQueue _messageQueue;

    public SettingsWindow()
    {
        InitializeComponent();
        
        // Initialize services and ViewModel
        _viewModel = new SettingsViewModel(
            TimeZoneManager.Instance,
            ThemeManager.Instance,
            SettingsManager.Instance,
            ClockService.Instance);
        
        // Set DataContext
        DataContext = _viewModel;
        
        // Initialize message queue for notifications
        // _messageQueue = new SnackbarMessageQueue();
        // UnsavedChangesSnackbar.MessageQueue = _messageQueue;
        
        // Subscribe to ViewModel events
        _viewModel.RequestClose += OnRequestClose;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        
        // Subscribe to theme changes for real-time preview
        ThemeManager.Instance.ThemeChanged += OnThemeChanged;
        ThemeManager.Instance.AccentColorChanged += OnAccentColorChanged;
        
        // Show unsaved changes notification if there are any
        if (_viewModel.HasUnsavedChanges)
        {
            // ShowUnsavedChangesMessage();
            System.Diagnostics.Debug.WriteLine("Has unsaved changes on startup");
        }
    }

    /// <summary>
    /// Handles ViewModel property changes to show notifications
    /// </summary>
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        try
        {
            if (e.PropertyName == nameof(SettingsViewModel.HasUnsavedChanges))
            {
                if (_viewModel.HasUnsavedChanges)
                {
                    // ShowUnsavedChangesMessage();
                }
            }
            else if (e.PropertyName == nameof(SettingsViewModel.IsDarkMode) ||
                     e.PropertyName == nameof(SettingsViewModel.SelectedAccentColor))
            {
                // Show preview notification - simplified for now
                System.Diagnostics.Debug.WriteLine("Theme preview applied");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error handling property change: {ex.Message}");
        }
    }

    /// <summary>
    /// Shows a notification about unsaved changes
    /// </summary>
    // private void ShowUnsavedChangesMessage()
    // {
    //     try
    //     {
    //         _messageQueue.Enqueue("You have unsaved changes", "SAVE NOW", () =>
    //         {
    //             _viewModel.SaveCommand.Execute(null);
    //         });
    //     }
    //     catch (Exception ex)
    //     {
    //         System.Diagnostics.Debug.WriteLine($"Error showing unsaved changes message: {ex.Message}");
    //     }
    // }

    /// <summary>
    /// Handles theme changes for real-time preview
    /// </summary>
    private void OnThemeChanged(object? sender, bool isDarkMode)
    {
        try
        {
            // Force refresh of Material Design resources
            if (Application.Current?.Resources != null)
            {
                // Update window background
                var background = FindResource("MaterialDesignPaper");
                Background = (System.Windows.Media.Brush)background;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating theme: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles accent color changes for real-time preview
    /// </summary>
    private void OnAccentColorChanged(object? sender, string accentColor)
    {
        try
        {
            // The ThemeManager should handle the accent color changes automatically
            // This event handler is here for any additional UI updates if needed
            System.Diagnostics.Debug.WriteLine($"Accent color changed to: {accentColor}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating accent color: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles ViewModel request to close the window
    /// </summary>
    private void OnRequestClose()
    {
        try
        {
            DialogResult = !_viewModel.HasUnsavedChanges;
            Close();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error closing settings window: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles window closing event to check for unsaved changes
    /// </summary>
    protected override void OnClosing(CancelEventArgs e)
    {
        try
        {
            // Check if there are unsaved changes
            if (!_viewModel.CanClose())
            {
                e.Cancel = true;
                return;
            }
            
            // Unsubscribe from events to prevent memory leaks
            _viewModel.RequestClose -= OnRequestClose;
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            ThemeManager.Instance.ThemeChanged -= OnThemeChanged;
            ThemeManager.Instance.AccentColorChanged -= OnAccentColorChanged;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during settings window closing: {ex.Message}");
        }
        
        base.OnClosing(e);
    }

    /// <summary>
    /// Handles window loaded event
    /// </summary>
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        
        try
        {
            // Center the window relative to owner
            if (Owner != null)
            {
                Left = Owner.Left + (Owner.Width - Width) / 2;
                Top = Owner.Top + (Owner.Height - Height) / 2;
                
                // Ensure window is on screen
                EnsureWindowOnScreen();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error positioning settings window: {ex.Message}");
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
            System.Diagnostics.Debug.WriteLine($"Error ensuring settings window on screen: {ex.Message}");
        }
    }

    /// <summary>
    /// Public method to show the settings window as a modal dialog
    /// </summary>
    /// <param name="owner">The owner window</param>
    /// <returns>True if settings were saved, false if cancelled</returns>
    public static bool? ShowSettingsDialog(Window owner)
    {
        try
        {
            var settingsWindow = new SettingsWindow
            {
                Owner = owner
            };
            
            return settingsWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error showing settings dialog: {ex.Message}");
            return false;
        }
    }
}
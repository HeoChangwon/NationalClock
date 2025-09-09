# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Development Commands

```bash
# Build the application
dotnet build

# Run the application
dotnet run

# Build for specific platform
dotnet build -p:Platform=x64

# Clean build artifacts
dotnet clean
```

## Architecture Overview

This is a **WPF Desktop Application** (.NET 8.0) implementing a multi-timezone world clock with Material Design UI. The project follows **MVVM architecture** with clean separation of concerns.

### Key Design Patterns

- **MVVM Pattern**: Uses CommunityToolkit.Mvvm with source generators (`[ObservableProperty]`, `[RelayCommand]`)
- **Singleton Services**: All core services (TimeZoneManager, ThemeManager, SettingsManager, ClockService) use thread-safe singleton pattern with `.Instance` property
- **Observer Pattern**: Event-driven updates for time changes, theme changes, and settings updates

### Core Service Architecture

```
Services (Singleton Pattern):
├── SettingsManager - JSON persistence in %LocalAppData%\NationalClock
├── TimeZoneManager - Manages enabled timezones and display order
├── ThemeManager - Material Design theme/color management
└── ClockService - Real-time updates via DispatcherTimer
```

### Project Structure

```
├── Models/         - Data models (Settings, TimeZoneInfo, ClockInfo)
├── Services/       - Business logic services (all singletons)  
├── ViewModels/     - MVVM ViewModels with CommunityToolkit.Mvvm
├── Views/          - XAML views and code-behind
├── Converters/     - WPF value converters
└── Resources/      - Application resources
```

## Material Design Integration

- Uses **MaterialDesignThemes 5.2.1** with BundledTheme
- Theme switching: Light/Dark mode with accent colors (Blue, Pink, etc.)
- All UI follows Material Design principles with Cards, proper spacing, and typography

## Settings Management

- **Storage**: `%LocalAppData%\NationalClock\settings.json`
- **Features**: Automatic backup, validation via `ValidateAndFix()`, immutable operations with `Clone()`
- **Scope**: Window state, timezone preferences, theme settings, time format, update intervals

## Key Dependencies

- **CommunityToolkit.Mvvm 8.4.0** - Modern MVVM with source generators
- **MaterialDesignThemes 5.2.1** - Google Material Design UI components  
- **System.Text.Json 9.0.8** - High-performance JSON serialization

## Real-Time Updates

- **Timer**: `DispatcherTimer` (1-second interval) in ClockService
- **Thread Safety**: All UI updates on main dispatcher thread
- **ObservableCollections**: Used for timezone lists with automatic UI binding

## Common Development Patterns

### Service Initialization
All services follow this pattern:
```csharp
private static readonly Lazy<ServiceName> _instance = new(() => new ServiceName());
public static ServiceName Instance => _instance.Value;
```

### ViewModel Commands
Uses CommunityToolkit.Mvvm source generators:
```csharp
[RelayCommand(CanExecute = nameof(CanExecuteMethod))]
private void CommandMethod() { }
```

### Settings Operations
Always use Clone() for immutable operations:
```csharp
var workingSettings = originalSettings.Clone();
// modify workingSettings
settingsManager.SaveSettings(workingSettings);
```

## Error Handling

- Global exception handling in `App.xaml.cs` with user dialogs
- All service methods include try-catch with debug logging
- Settings validation and recovery mechanisms
- Graceful degradation on service failures

## Material Design Resources

If you encounter missing Material Design styles:
1. Verify `BundledTheme` is properly referenced in App.xaml
2. Use basic WPF styles as fallback rather than specific MaterialDesign styles
3. Common working styles: Basic Button, ComboBox, ToggleButton (avoid MaterialDesignComboBox, MaterialDesignSwitchToggleButton)

## Threading Considerations

- UI updates must use `Application.Current.Dispatcher.Invoke()`
- Service operations are thread-safe via locking mechanisms
- Timer operations run on UI thread via DispatcherTimer
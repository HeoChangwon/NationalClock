using NationalClock.Models;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace NationalClock.Services;

/// <summary>
/// Service for managing clock updates and time calculations
/// Uses DispatcherTimer for regular time updates and handles format conversions
/// </summary>
public sealed class ClockService : IDisposable
{
    private static readonly Lazy<ClockService> _instance = new(() => new ClockService());
    private readonly DispatcherTimer _timer;
    private readonly ObservableCollection<ClockInfo> _clocks;
    private readonly TimeZoneManager _timeZoneManager;
    private readonly object _lock = new();
    private bool _is24HourFormat = true;
    private bool _disposed = false;
    private int _updateIntervalSeconds = 1;

    /// <summary>
    /// Gets the singleton instance of ClockService
    /// </summary>
    public static ClockService Instance => _instance.Value;

    /// <summary>
    /// Event fired when clocks are updated
    /// </summary>
    public event EventHandler? ClocksUpdated;

    /// <summary>
    /// Event fired when time format changes
    /// </summary>
    public event EventHandler<bool>? TimeFormatChanged;

    /// <summary>
    /// Private constructor to prevent external instantiation
    /// </summary>
    private ClockService()
    {
        _clocks = new ObservableCollection<ClockInfo>();
        _timeZoneManager = TimeZoneManager.Instance;
        
        // Initialize timer with 1-second interval
        _timer = new DispatcherTimer(DispatcherPriority.Normal)
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += OnTimerTick;
        
        InitializeClocks();
    }

    /// <summary>
    /// Gets the observable collection of clock instances
    /// </summary>
    public ObservableCollection<ClockInfo> Clocks
    {
        get
        {
            lock (_lock)
            {
                return _clocks;
            }
        }
    }

    /// <summary>
    /// Gets or sets whether to use 24-hour time format
    /// </summary>
    public bool Is24HourFormat
    {
        get => _is24HourFormat;
        set
        {
            if (_is24HourFormat != value)
            {
                _is24HourFormat = value;
                UpdateTimeFormat(value);
                TimeFormatChanged?.Invoke(this, value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the update interval in seconds
    /// </summary>
    public int UpdateIntervalSeconds
    {
        get => _updateIntervalSeconds;
        set
        {
            if (value >= 1 && _updateIntervalSeconds != value)
            {
                _updateIntervalSeconds = value;
                _timer.Interval = TimeSpan.FromSeconds(value);
            }
        }
    }

    /// <summary>
    /// Gets whether the timer is currently running
    /// </summary>
    public bool IsRunning => _timer.IsEnabled;

    /// <summary>
    /// Starts the clock update timer
    /// </summary>
    public void Start()
    {
        if (!_disposed && !_timer.IsEnabled)
        {
            UpdateAllClocks(); // Update immediately
            _timer.Start();
        }
    }

    /// <summary>
    /// Stops the clock update timer
    /// </summary>
    public void Stop()
    {
        if (_timer.IsEnabled)
        {
            _timer.Stop();
        }
    }

    /// <summary>
    /// Initializes clocks based on enabled timezones
    /// </summary>
    private void InitializeClocks()
    {
        lock (_lock)
        {
            _clocks.Clear();
            var enabledTimeZones = _timeZoneManager.EnabledTimeZones;
            
            foreach (var timeZone in enabledTimeZones)
            {
                var clockInfo = new ClockInfo(timeZone)
                {
                    Is24HourFormat = _is24HourFormat
                };
                _clocks.Add(clockInfo);
            }
        }
    }

    /// <summary>
    /// Updates the enabled clocks based on current timezone settings
    /// </summary>
    public void RefreshClocks()
    {
        lock (_lock)
        {
            var enabledTimeZones = _timeZoneManager.EnabledTimeZones.ToList();
            var currentClockTimeZoneIds = _clocks.Select(c => c.TimeZone.Id).ToHashSet();
            var enabledTimeZoneIds = enabledTimeZones.Select(tz => tz.Id).ToHashSet();

            // Remove clocks that are no longer enabled
            var clocksToRemove = _clocks.Where(c => !enabledTimeZoneIds.Contains(c.TimeZone.Id)).ToList();
            foreach (var clock in clocksToRemove)
            {
                _clocks.Remove(clock);
            }

            // Add clocks for newly enabled timezones
            foreach (var timeZone in enabledTimeZones)
            {
                if (!currentClockTimeZoneIds.Contains(timeZone.Id))
                {
                    var clockInfo = new ClockInfo(timeZone)
                    {
                        Is24HourFormat = _is24HourFormat
                    };
                    
                    // Insert at correct position based on display order
                    var insertIndex = 0;
                    for (int i = 0; i < _clocks.Count; i++)
                    {
                        if (_clocks[i].TimeZone.DisplayOrder > timeZone.DisplayOrder)
                            break;
                        insertIndex = i + 1;
                    }
                    
                    _clocks.Insert(insertIndex, clockInfo);
                }
            }

            // Reorder existing clocks based on current display order
            var orderedClocks = _clocks.OrderBy(c => c.TimeZone.DisplayOrder).ToList();
            _clocks.Clear();
            foreach (var clock in orderedClocks)
            {
                _clocks.Add(clock);
            }
        }
        
        // Update immediately after refresh
        UpdateAllClocks();
    }

    /// <summary>
    /// Timer tick event handler that updates all clocks
    /// </summary>
    private void OnTimerTick(object? sender, EventArgs e)
    {
        try
        {
            UpdateAllClocks();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating clocks: {ex.Message}");
            // Log error but continue running - don't crash the timer
        }
    }

    /// <summary>
    /// Updates the time for all clock instances
    /// </summary>
    public void UpdateAllClocks()
    {
        lock (_lock)
        {
            foreach (var clock in _clocks)
            {
                try
                {
                    clock.UpdateTime();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error updating clock for {clock.TimeZone.DisplayName}: {ex.Message}");
                    // Continue with other clocks even if one fails
                }
            }
        }
        
        try
        {
            ClocksUpdated?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error invoking ClocksUpdated event: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates the time format for all clocks
    /// </summary>
    /// <param name="is24Hour">True for 24-hour format, false for 12-hour format</param>
    private void UpdateTimeFormat(bool is24Hour)
    {
        lock (_lock)
        {
            foreach (var clock in _clocks)
            {
                clock.SetTimeFormat(is24Hour);
            }
        }
    }

    /// <summary>
    /// Adds a clock for the specified timezone
    /// </summary>
    /// <param name="timeZoneInfo">The timezone to add</param>
    /// <returns>True if added successfully</returns>
    public bool AddClock(Models.TimeZoneInfo timeZoneInfo)
    {
        if (timeZoneInfo == null) return false;

        lock (_lock)
        {
            // Check if clock already exists
            if (_clocks.Any(c => c.TimeZone.Id == timeZoneInfo.Id))
                return false;

            var clockInfo = new ClockInfo(timeZoneInfo)
            {
                Is24HourFormat = _is24HourFormat
            };
            
            // Insert at correct position based on display order
            var insertIndex = 0;
            for (int i = 0; i < _clocks.Count; i++)
            {
                if (_clocks[i].TimeZone.DisplayOrder > timeZoneInfo.DisplayOrder)
                    break;
                insertIndex = i + 1;
            }
            
            _clocks.Insert(insertIndex, clockInfo);
            clockInfo.UpdateTime(); // Update immediately
            return true;
        }
    }

    /// <summary>
    /// Removes a clock by timezone ID
    /// </summary>
    /// <param name="timeZoneId">The timezone ID to remove</param>
    /// <returns>True if removed successfully</returns>
    public bool RemoveClock(int timeZoneId)
    {
        lock (_lock)
        {
            var clock = _clocks.FirstOrDefault(c => c.TimeZone.Id == timeZoneId);
            if (clock != null)
            {
                return _clocks.Remove(clock);
            }
            return false;
        }
    }

    /// <summary>
    /// Gets a clock by timezone ID
    /// </summary>
    /// <param name="timeZoneId">The timezone ID</param>
    /// <returns>ClockInfo instance or null if not found</returns>
    public ClockInfo? GetClock(int timeZoneId)
    {
        lock (_lock)
        {
            return _clocks.FirstOrDefault(c => c.TimeZone.Id == timeZoneId);
        }
    }

    /// <summary>
    /// Converts time from 24-hour to 12-hour format
    /// </summary>
    /// <param name="time24">Time in 24-hour format (e.g., "14:30:45")</param>
    /// <returns>Time in 12-hour format (e.g., "2:30:45 PM")</returns>
    public static string Convert24To12Hour(string time24)
    {
        if (DateTime.TryParseExact(time24, "HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out DateTime dateTime))
        {
            return dateTime.ToString("h:mm:ss tt", System.Globalization.CultureInfo.InvariantCulture);
        }
        return time24; // Return original if parsing fails
    }

    /// <summary>
    /// Converts time from 12-hour to 24-hour format
    /// </summary>
    /// <param name="time12">Time in 12-hour format (e.g., "2:30:45 PM")</param>
    /// <returns>Time in 24-hour format (e.g., "14:30:45")</returns>
    public static string Convert12To24Hour(string time12)
    {
        if (DateTime.TryParseExact(time12, "h:mm:ss tt", null, System.Globalization.DateTimeStyles.None, out DateTime dateTime))
        {
            return dateTime.ToString("HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        }
        return time12; // Return original if parsing fails
    }

    /// <summary>
    /// Forces an immediate update of all clocks
    /// </summary>
    public void ForceUpdate()
    {
        UpdateAllClocks();
    }

    /// <summary>
    /// Disposes of the timer and releases resources
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            Stop();
            // DispatcherTimer doesn't implement IDisposable, just stop it
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer to ensure timer is disposed
    /// </summary>
    ~ClockService()
    {
        Dispose();
    }
}
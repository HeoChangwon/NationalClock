using CommunityToolkit.Mvvm.ComponentModel;
using System.Globalization;

namespace NationalClock.Models;

/// <summary>
/// Model representing a clock instance with real-time time updates
/// Implements INotifyPropertyChanged for data binding support
/// </summary>
public partial class ClockInfo : ObservableObject
{
    [ObservableProperty]
    private TimeZoneInfo _timeZone;

    [ObservableProperty]
    private DateTime _currentTime;

    [ObservableProperty]
    private bool _is24HourFormat = true;

    /// <summary>
    /// Constructor that creates a clock for the specified timezone
    /// </summary>
    /// <param name="timeZone">The timezone information for this clock</param>
    public ClockInfo(TimeZoneInfo timeZone)
    {
        _timeZone = timeZone;
        _currentTime = timeZone.CurrentTime;
    }

    /// <summary>
    /// Gets the formatted time string based on the current format setting
    /// </summary>
    public string FormattedTime
    {
        get
        {
            if (Is24HourFormat)
            {
                return CurrentTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            }
            else
            {
                return CurrentTime.ToString("h:mm:ss tt", CultureInfo.InvariantCulture);
            }
        }
    }

    /// <summary>
    /// Gets the formatted date string for the current time
    /// </summary>
    public string DateString
    {
        get
        {
            return CurrentTime.ToString("dddd, MMMM dd, yyyy", CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// Gets the short date string for compact display
    /// </summary>
    public string ShortDateString
    {
        get
        {
            return CurrentTime.ToString("MMM dd", CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// Gets the timezone offset string (e.g., "+09:00")
    /// </summary>
    public string TimeZoneOffsetString
    {
        get
        {
            var systemTz = TimeZone.SystemTimeZone;
            if (systemTz != null)
            {
                var offset = systemTz.GetUtcOffset(CurrentTime);
                var sign = offset >= TimeSpan.Zero ? "+" : "-";
                return $"{sign}{Math.Abs(offset.Hours):D2}:{Math.Abs(offset.Minutes):D2}";
            }
            return "+00:00";
        }
    }

    /// <summary>
    /// Updates the current time for this clock
    /// This method should be called regularly by the ClockService
    /// </summary>
    public void UpdateTime()
    {
        var previousTime = CurrentTime;
        CurrentTime = TimeZone.CurrentTime;

        // Only notify property changes if time format could have changed
        if (previousTime.Hour != CurrentTime.Hour || 
            previousTime.Minute != CurrentTime.Minute || 
            previousTime.Second != CurrentTime.Second)
        {
            OnPropertyChanged(nameof(FormattedTime));
        }

        // Notify date changes if day changed
        if (previousTime.Date != CurrentTime.Date)
        {
            OnPropertyChanged(nameof(DateString));
            OnPropertyChanged(nameof(ShortDateString));
        }

        // Notify offset changes if DST changed
        if (previousTime.Hour != CurrentTime.Hour)
        {
            OnPropertyChanged(nameof(TimeZoneOffsetString));
        }
    }

    /// <summary>
    /// Changes the time format and notifies UI of the change
    /// </summary>
    /// <param name="is24Hour">True for 24-hour format, false for 12-hour format</param>
    public void SetTimeFormat(bool is24Hour)
    {
        if (Is24HourFormat != is24Hour)
        {
            Is24HourFormat = is24Hour;
            OnPropertyChanged(nameof(FormattedTime));
        }
    }

    /// <summary>
    /// Gets display text for the timezone (emoji + name)
    /// </summary>
    public string DisplayText => $"{TimeZone.FlagEmoji} {TimeZone.DisplayName}";

    /// <summary>
    /// Gets whether this timezone is in daylight saving time
    /// </summary>
    public bool IsDaylightSavingTime
    {
        get
        {
            var systemTz = TimeZone.SystemTimeZone;
            return systemTz?.IsDaylightSavingTime(CurrentTime) ?? false;
        }
    }

    public override string ToString()
    {
        return $"{TimeZone.DisplayName}: {FormattedTime}";
    }
}
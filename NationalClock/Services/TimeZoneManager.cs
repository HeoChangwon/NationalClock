using NationalClock.Models;
using System.Collections.ObjectModel;

namespace NationalClock.Services;

/// <summary>
/// Service for managing timezone data and operations
/// Implements Singleton pattern for global access
/// </summary>
public sealed class TimeZoneManager
{
    private static readonly Lazy<TimeZoneManager> _instance = new(() => new TimeZoneManager());
    private readonly List<Models.TimeZoneInfo> _allTimeZones;
    private readonly object _lock = new();

    /// <summary>
    /// Gets the singleton instance of TimeZoneManager
    /// </summary>
    public static TimeZoneManager Instance => _instance.Value;

    /// <summary>
    /// Private constructor to prevent external instantiation
    /// </summary>
    private TimeZoneManager()
    {
        _allTimeZones = new List<Models.TimeZoneInfo>();
        InitializeDefaultTimeZones();
    }

    /// <summary>
    /// Gets all available timezones
    /// </summary>
    public IReadOnlyList<Models.TimeZoneInfo> AllTimeZones
    {
        get
        {
            lock (_lock)
            {
                return _allTimeZones.AsReadOnly();
            }
        }
    }

    /// <summary>
    /// Gets all enabled timezones ordered by DisplayOrder
    /// </summary>
    public IEnumerable<Models.TimeZoneInfo> EnabledTimeZones
    {
        get
        {
            lock (_lock)
            {
                return _allTimeZones
                    .Where(tz => tz.IsEnabled)
                    .OrderBy(tz => tz.DisplayOrder)
                    .ToList();
            }
        }
    }

    /// <summary>
    /// Initializes the default timezone data for Korea, Michigan (US), and Poland
    /// </summary>
    private void InitializeDefaultTimeZones()
    {
        var defaultTimeZones = new List<Models.TimeZoneInfo>
        {
            // Korea (Seoul)
            new Models.TimeZoneInfo(
                id: 1,
                displayName: "Seoul, South Korea",
                timeZoneId: "Korea Standard Time",
                flagEmoji: "🇰🇷",
                isEnabled: true,
                displayOrder: 1
            ),

            // Michigan, USA (Detroit)
            new Models.TimeZoneInfo(
                id: 2,
                displayName: "Michigan, United States",
                timeZoneId: "Eastern Standard Time",
                flagEmoji: "🇺🇸",
                isEnabled: true,
                displayOrder: 2
            ),

            // Poland (Warsaw)
            new Models.TimeZoneInfo(
                id: 3,
                displayName: "Warsaw, Poland",
                timeZoneId: "Central European Standard Time",
                flagEmoji: "🇵🇱",
                isEnabled: true,
                displayOrder: 3
            ),

            // Additional common timezones (disabled by default)
            new Models.TimeZoneInfo(
                id: 4,
                displayName: "London, United Kingdom",
                timeZoneId: "GMT Standard Time",
                flagEmoji: "🇬🇧",
                isEnabled: false,
                displayOrder: 4
            ),

            new Models.TimeZoneInfo(
                id: 5,
                displayName: "Tokyo, Japan",
                timeZoneId: "Tokyo Standard Time",
                flagEmoji: "🇯🇵",
                isEnabled: false,
                displayOrder: 5
            ),

            new Models.TimeZoneInfo(
                id: 6,
                displayName: "New York, United States",
                timeZoneId: "Eastern Standard Time",
                flagEmoji: "🇺🇸",
                isEnabled: false,
                displayOrder: 6
            ),

            new Models.TimeZoneInfo(
                id: 7,
                displayName: "Los Angeles, United States",
                timeZoneId: "Pacific Standard Time",
                flagEmoji: "🇺🇸",
                isEnabled: false,
                displayOrder: 7
            ),

            new Models.TimeZoneInfo(
                id: 8,
                displayName: "Berlin, Germany",
                timeZoneId: "W. Europe Standard Time",
                flagEmoji: "🇩🇪",
                isEnabled: false,
                displayOrder: 8
            ),

            new Models.TimeZoneInfo(
                id: 9,
                displayName: "Paris, France",
                timeZoneId: "Romance Standard Time",
                flagEmoji: "🇫🇷",
                isEnabled: false,
                displayOrder: 9
            ),

            new Models.TimeZoneInfo(
                id: 10,
                displayName: "Sydney, Australia",
                timeZoneId: "AUS Eastern Standard Time",
                flagEmoji: "🇦🇺",
                isEnabled: false,
                displayOrder: 10
            )
        };

        lock (_lock)
        {
            _allTimeZones.AddRange(defaultTimeZones);
        }
    }

    /// <summary>
    /// Gets a timezone by its ID
    /// </summary>
    /// <param name="id">The timezone ID</param>
    /// <returns>TimeZone info or null if not found</returns>
    public Models.TimeZoneInfo? GetTimeZoneById(int id)
    {
        lock (_lock)
        {
            return _allTimeZones.FirstOrDefault(tz => tz.Id == id);
        }
    }

    /// <summary>
    /// Gets timezones by their IDs in the specified order
    /// </summary>
    /// <param name="ids">List of timezone IDs</param>
    /// <returns>List of timezone infos in the order of the IDs</returns>
    public List<Models.TimeZoneInfo> GetTimeZonesByIds(IEnumerable<int> ids)
    {
        lock (_lock)
        {
            var result = new List<Models.TimeZoneInfo>();
            foreach (var id in ids)
            {
                var timeZone = _allTimeZones.FirstOrDefault(tz => tz.Id == id);
                if (timeZone != null)
                {
                    result.Add(timeZone);
                }
            }
            return result;
        }
    }

    /// <summary>
    /// Adds a new timezone to the collection
    /// </summary>
    /// <param name="timeZone">The timezone to add</param>
    /// <returns>True if added successfully, false if ID already exists</returns>
    public bool AddTimeZone(Models.TimeZoneInfo timeZone)
    {
        if (timeZone == null) return false;

        lock (_lock)
        {
            if (_allTimeZones.Any(tz => tz.Id == timeZone.Id))
                return false;

            _allTimeZones.Add(timeZone);
            return true;
        }
    }

    /// <summary>
    /// Removes a timezone by its ID
    /// </summary>
    /// <param name="id">The timezone ID to remove</param>
    /// <returns>True if removed successfully, false if not found</returns>
    public bool RemoveTimeZone(int id)
    {
        lock (_lock)
        {
            var timeZone = _allTimeZones.FirstOrDefault(tz => tz.Id == id);
            if (timeZone != null)
            {
                return _allTimeZones.Remove(timeZone);
            }
            return false;
        }
    }

    /// <summary>
    /// Enables or disables a timezone
    /// </summary>
    /// <param name="id">The timezone ID</param>
    /// <param name="isEnabled">Whether to enable or disable</param>
    /// <returns>True if operation successful, false if timezone not found</returns>
    public bool SetTimeZoneEnabled(int id, bool isEnabled)
    {
        lock (_lock)
        {
            var timeZone = _allTimeZones.FirstOrDefault(tz => tz.Id == id);
            if (timeZone != null)
            {
                timeZone.IsEnabled = isEnabled;
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Updates the display order of multiple timezones
    /// </summary>
    /// <param name="timeZoneOrders">Dictionary of timezone ID to display order</param>
    public void UpdateDisplayOrders(Dictionary<int, int> timeZoneOrders)
    {
        lock (_lock)
        {
            foreach (var kvp in timeZoneOrders)
            {
                var timeZone = _allTimeZones.FirstOrDefault(tz => tz.Id == kvp.Key);
                if (timeZone != null)
                {
                    timeZone.DisplayOrder = kvp.Value;
                }
            }
        }
    }

    /// <summary>
    /// Moves a timezone up in the display order
    /// </summary>
    /// <param name="id">The timezone ID to move up</param>
    /// <returns>True if moved successfully</returns>
    public bool MoveTimeZoneUp(int id)
    {
        lock (_lock)
        {
            var timeZone = _allTimeZones.FirstOrDefault(tz => tz.Id == id);
            if (timeZone == null) return false;

            var enabledTimeZones = _allTimeZones
                .Where(tz => tz.IsEnabled)
                .OrderBy(tz => tz.DisplayOrder)
                .ToList();

            var currentIndex = enabledTimeZones.IndexOf(timeZone);
            if (currentIndex <= 0) return false; // Already at top or not found

            // Swap display orders
            var previousTimeZone = enabledTimeZones[currentIndex - 1];
            (timeZone.DisplayOrder, previousTimeZone.DisplayOrder) = (previousTimeZone.DisplayOrder, timeZone.DisplayOrder);

            return true;
        }
    }

    /// <summary>
    /// Moves a timezone down in the display order
    /// </summary>
    /// <param name="id">The timezone ID to move down</param>
    /// <returns>True if moved successfully</returns>
    public bool MoveTimeZoneDown(int id)
    {
        lock (_lock)
        {
            var timeZone = _allTimeZones.FirstOrDefault(tz => tz.Id == id);
            if (timeZone == null) return false;

            var enabledTimeZones = _allTimeZones
                .Where(tz => tz.IsEnabled)
                .OrderBy(tz => tz.DisplayOrder)
                .ToList();

            var currentIndex = enabledTimeZones.IndexOf(timeZone);
            if (currentIndex < 0 || currentIndex >= enabledTimeZones.Count - 1) return false; // At bottom or not found

            // Swap display orders
            var nextTimeZone = enabledTimeZones[currentIndex + 1];
            (timeZone.DisplayOrder, nextTimeZone.DisplayOrder) = (nextTimeZone.DisplayOrder, timeZone.DisplayOrder);

            return true;
        }
    }

    /// <summary>
    /// Updates the enabled timezones based on settings
    /// </summary>
    /// <param name="enabledIds">List of timezone IDs that should be enabled</param>
    public void UpdateEnabledTimeZones(IEnumerable<int> enabledIds)
    {
        lock (_lock)
        {
            var enabledSet = new HashSet<int>(enabledIds);

            foreach (var timeZone in _allTimeZones)
            {
                timeZone.IsEnabled = enabledSet.Contains(timeZone.Id);
            }

            // Update display orders to match the order in enabledIds
            var idList = enabledIds.ToList();
            for (int i = 0; i < idList.Count; i++)
            {
                var timeZone = _allTimeZones.FirstOrDefault(tz => tz.Id == idList[i]);
                if (timeZone != null)
                {
                    timeZone.DisplayOrder = i + 1;
                }
            }
        }
    }

    /// <summary>
    /// Gets the current time for a specific timezone
    /// </summary>
    /// <param name="timeZoneId">The timezone ID</param>
    /// <returns>Current DateTime in the specified timezone, or DateTime.Now if not found</returns>
    public DateTime GetCurrentTimeForTimeZone(int timeZoneId)
    {
        var timeZone = GetTimeZoneById(timeZoneId);
        return timeZone?.CurrentTime ?? DateTime.Now;
    }

    /// <summary>
    /// Validates that all default timezones have valid system timezone IDs
    /// </summary>
    /// <returns>List of invalid timezone IDs</returns>
    public List<string> ValidateTimeZones()
    {
        var invalidTimeZones = new List<string>();

        lock (_lock)
        {
            foreach (var timeZone in _allTimeZones)
            {
                try
                {
                    System.TimeZoneInfo.FindSystemTimeZoneById(timeZone.TimeZoneId);
                }
                catch
                {
                    invalidTimeZones.Add($"{timeZone.DisplayName} ({timeZone.TimeZoneId})");
                }
            }
        }

        return invalidTimeZones;
    }
}
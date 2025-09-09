using System.Text.Json.Serialization;

namespace NationalClock.Models;

/// <summary>
/// Model representing timezone information for the National Clock application
/// Contains all necessary properties for displaying and managing timezone clocks
/// </summary>
public class TimeZoneInfo
{
    /// <summary>
    /// Unique identifier for this timezone entry
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Display name for the timezone (e.g., "Seoul, South Korea")
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// System timezone identifier (e.g., "Asia/Seoul")
    /// </summary>
    public string TimeZoneId { get; set; } = string.Empty;

    /// <summary>
    /// Unicode flag emoji for the country/region (e.g., "🇰🇷")
    /// </summary>
    public string FlagEmoji { get; set; } = string.Empty;

    /// <summary>
    /// Path to flag image file (optional, can be null if using emoji)
    /// </summary>
    public string? FlagImagePath { get; set; }

    /// <summary>
    /// Whether this timezone is currently enabled/visible in the main window
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Display order in the main window (lower numbers appear first)
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public TimeZoneInfo()
    {
    }

    /// <summary>
    /// Constructor with all parameters for easy initialization
    /// </summary>
    /// <param name="id">Unique identifier</param>
    /// <param name="displayName">Display name</param>
    /// <param name="timeZoneId">System timezone identifier</param>
    /// <param name="flagEmoji">Unicode flag emoji</param>
    /// <param name="flagImagePath">Optional flag image path</param>
    /// <param name="isEnabled">Whether enabled by default</param>
    /// <param name="displayOrder">Display order</param>
    public TimeZoneInfo(int id, string displayName, string timeZoneId, string flagEmoji, 
        string? flagImagePath = null, bool isEnabled = true, int displayOrder = 0)
    {
        Id = id;
        DisplayName = displayName;
        TimeZoneId = timeZoneId;
        FlagEmoji = flagEmoji;
        FlagImagePath = flagImagePath;
        IsEnabled = isEnabled;
        DisplayOrder = displayOrder;
    }

    /// <summary>
    /// Gets the actual System.TimeZoneInfo object for this timezone
    /// </summary>
    /// <returns>System.TimeZoneInfo object or null if not found</returns>
    [JsonIgnore]
    public System.TimeZoneInfo? SystemTimeZone
    {
        get
        {
            try
            {
                return System.TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Gets the current time in this timezone
    /// </summary>
    /// <returns>Current DateTime in this timezone or DateTime.Now if timezone not found</returns>
    [JsonIgnore]
    public DateTime CurrentTime
    {
        get
        {
            var tz = SystemTimeZone;
            return tz != null ? System.TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz) : DateTime.Now;
        }
    }

    public override string ToString()
    {
        return $"{FlagEmoji} {DisplayName}";
    }

    public override bool Equals(object? obj)
    {
        return obj is TimeZoneInfo other && Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
using System.Reflection;

namespace NationalClock;

public static class VersionInfo
{
    /// <summary>
    /// Gets the application name from assembly metadata or fallback
    /// </summary>
    public static string ApplicationName => GetAssemblyMetadata("ApplicationDisplayName") ?? "NationalClock";
    
    /// <summary>
    /// Gets the display version from assembly metadata
    /// This will automatically reflect in Windows installer and file properties
    /// </summary>
    public static string Version => GetAssemblyMetadata("DisplayVersion") ?? GetAssemblyVersion();
    
    /// <summary>
    /// Gets the build date from assembly metadata
    /// </summary>
    public static string BuildDate => GetAssemblyMetadata("BuildDateTime") ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm");
    
    public static string FullTitle => $"{ApplicationName} v{Version} (Build: {BuildDate})";
    
    /// <summary>
    /// Gets assembly metadata by key
    /// </summary>
    private static string? GetAssemblyMetadata(string key)
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var attributes = assembly.GetCustomAttributes<AssemblyMetadataAttribute>();
            return attributes.FirstOrDefault(x => x.Key == key)?.Value;
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Fallback method to get assembly version
    /// </summary>
    private static string GetAssemblyVersion()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            // First try to get InformationalVersion for display
            var infoVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            if (!string.IsNullOrEmpty(infoVersion))
                return infoVersion;
            
            // Fallback to AssemblyVersion
            return assembly.GetName().Version?.ToString() ?? "1.0.0.0";
        }
        catch
        {
            return "1.0.0.0";
        }
    }
}
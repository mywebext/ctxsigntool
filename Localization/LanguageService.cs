namespace CtxSignTool.Localization;
/// <summary>
/// Provides methods for loading localized string resources and retrieving string values based on keys.
/// </summary>
/// <remarks>This static class manages a dictionary of localized strings, allowing for dynamic loading from a JSON
/// file and retrieval of values with fallback options for different languages.</remarks>
public static class LanguageService
{
    private static Dictionary<string, string> _strings = new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// Loads localized string resources from a JSON file at the specified path.
    /// </summary>
    /// <remarks>If the file at the specified path does not exist or is null, the method initializes the
    /// resource dictionary to an empty state. The JSON file is expected to represent a dictionary of string keys and
    /// values, typically used for localization.</remarks>
    /// <param name="path">The path to the JSON file containing the serialized dictionary of string key-value pairs. Must be a valid file
    /// path; if null or the file does not exist, an empty dictionary is initialized.</param>
    public static void Load(string path)
    {
        if (Null(path) || !File.Exists(path))
        {
            _strings = new(StringComparer.OrdinalIgnoreCase);
            return;
        }

        var json = File.ReadAllText(path, Encoding.UTF8);
        _strings = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
            ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
    /// <summary>
    /// Retrieves the string value associated with the specified key, or returns a fallback value if the key is not
    /// found or its value is null.
    /// </summary>
    /// <remarks>If the key does not exist in the internal dictionary or its value is null, the method returns
    /// the provided fallback value. If the fallback value is also null, the method returns the key itself. This allows
    /// for flexible handling of missing or null values in localization scenarios.</remarks>
    /// <param name="key">The key whose associated string value is to be retrieved. This parameter cannot be null.</param>
    /// <param name="fallback">The value to return if the key is not found or its associated value is null. Defaults to an empty string if not
    /// specified.</param>
    /// <returns>The string value associated with the specified key if found and not null; otherwise, returns the fallback value,
    /// or the key itself if the fallback is null.</returns>
    public static string T(string key, string fallback = "")
    {
        if (_strings.TryGetValue(key, out var value) && !Null(value))
            return value;

        return Null(fallback) ? key : fallback;
    }
}

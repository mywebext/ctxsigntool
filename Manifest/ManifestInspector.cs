namespace CtxSignTool.Manifest;

/// <summary>
/// Provides helper methods for inspecting a manifest file and locating
/// the recorded hash for a file relative to a specified root directory.
/// </summary>
public static class ManifestInspector
{
    /// <summary>
    /// Attempts to find the recorded hash for the specified file within the manifest JSON file.
    /// </summary>
    /// <param name="manifestPath">
    /// The path to the manifest JSON file to inspect.
    /// </param>
    /// <param name="filePath">
    /// The path to the file whose manifest hash should be located.
    /// </param>
    /// <param name="rootDir">
    /// The root directory used to compute the file's manifest-relative path.
    /// </param>
    /// <param name="hash">
    /// When this method returns, contains the matching hash value if found; otherwise an empty string.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a matching file entry and hash were found in the manifest;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryFindHash(string manifestPath, string filePath, string rootDir, out string hash)
    {
        hash = string.Empty;
        using var doc = JsonDocument.Parse(File.ReadAllText(manifestPath, Encoding.UTF8));
        string relative = NormalizePath(Path.GetRelativePath(rootDir, filePath)).Replace('\\', '/');
        return TryFindHash(doc.RootElement, relative, out hash);
    }

    private static bool TryFindHash(JsonElement element, string relativePath, out string hash)
    {
        hash = string.Empty;

        if (element.ValueKind == JsonValueKind.Object)
        {
            if (element.TryGetProperty("files", out var files))
            {
                if (TryFindHash(files, relativePath, out hash))
                    return true;
            }

            foreach (var prop in element.EnumerateObject())
            {
                if (prop.Value.ValueKind == JsonValueKind.String && PathsEqual(prop.Name, relativePath))
                {
                    hash = prop.Value.GetString() ?? string.Empty;
                    return !Null(hash);
                }

                if (prop.Value.ValueKind == JsonValueKind.Object)
                {
                    string path = GetString(prop.Value, "path", "file", "name", "relativePath");
                    string value = GetString(prop.Value, "sha256", "hash", "digest");
                    if (!Null(path) && !Null(value) && PathsEqual(path, relativePath))
                    {
                        hash = value;
                        return true;
                    }
                }
            }
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Object)
                {
                    string path = GetString(item, "path", "file", "name", "relativePath");
                    string value = GetString(item, "sha256", "hash", "digest");
                    if (!Null(path) && !Null(value) && PathsEqual(path, relativePath))
                    {
                        hash = value;
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private static string GetString(JsonElement obj, params string[] names)
    {
        foreach (var name in names)
        {
            if (obj.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
                return value.GetString() ?? string.Empty;
        }

        return string.Empty;
    }

    private static bool PathsEqual(string a, string b)
    {
        a = NormalizePath(a).Replace('\\', '/').TrimStart('/');
        b = NormalizePath(b).Replace('\\', '/').TrimStart('/');
        return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }
}
namespace CtxSignTool.Output;

/// <summary>
/// Provides helper methods for writing command output to files,
/// supporting optional file output paths used by CLI commands.
/// </summary>
public static class FileOutput
{
    /// <summary>
    /// Writes the specified content to a file when a destination path is provided.
    /// </summary>
    /// <param name="path">
    /// The destination file path. If null or empty, no file is written and the method returns without performing any action.
    /// </param>
    /// <param name="content">
    /// The text content to write to the specified file.
    /// </param>
    public static void MaybeWrite(string path, string content)
    {
        if (Null(path))
            return;

        File.WriteAllText(path, content ?? string.Empty, Encoding.UTF8);
    }
}
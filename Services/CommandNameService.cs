namespace CtxSignTool.Services;

/// <summary>
/// Provides helper methods for resolving the command name used to invoke the tool.
/// </summary>
public static class CommandNameService
{
    /// <summary>
    /// Determines the command name used to launch the current process.
    /// </summary>
    /// <returns>
    /// The resolved command name used for help text and user-facing output.
    /// Returns <c>ctxsigntool</c> if a command name cannot be determined.
    /// </returns>
    public static string GetCommandName()
    {
        string host = Path.GetFileNameWithoutExtension(Environment.ProcessPath ?? string.Empty);
        string invoked = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs().FirstOrDefault() ?? string.Empty);
        bool dotnetHost = string.Equals(host, "dotnet", StringComparison.OrdinalIgnoreCase);
        string cmd = dotnetHost ? invoked : host;
        return Null(cmd) ? "ctxsigntool" : cmd;
    }
}

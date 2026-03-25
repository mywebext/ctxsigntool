namespace CtxSignTool.Services;

/// <summary>
/// Provides helper methods for retrieving version information for CtxSignTool
/// and the underlying CtxSignLib assembly.
/// </summary>
public static class VersionService
{
    /// <summary>
    /// Gets the version string for the currently executing CtxSignTool assembly.
    /// </summary>
    /// <returns>
    /// The informational version of the tool assembly when available; otherwise the
    /// assembly version or <c>unknown</c>.
    /// </returns>
    public static string GetToolVersion()
    {
        var asm = Assembly.GetExecutingAssembly();
        var info = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!Null(info)) return info!;
        return asm.GetName().Version?.ToString() ?? "unknown";
    }

    /// <summary>
    /// Gets the version string for the referenced CtxSignLib assembly.
    /// </summary>
    /// <returns>
    /// The informational version of the CtxSignLib assembly when available;
    /// otherwise <c>unknown</c>.
    /// </returns>
    public static string GetLibraryVersion()
    {
        return typeof(CtxSignlib.Functions)
            .Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "unknown";
    }
}
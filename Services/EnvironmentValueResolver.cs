namespace CtxSignTool.Services;

/// <summary>
/// Provides helper methods for resolving command values that may reference environment variables.
/// </summary>
public static class EnvironmentValueResolver
{
    /// <summary>
    /// Resolves a value that may use the <c>env:</c> prefix to reference an environment variable.
    /// </summary>
    /// <param name="value">
    /// The input value to resolve. If the value starts with <c>env:</c>, the referenced
    /// environment variable is read and returned.
    /// </param>
    /// <returns>
    /// The resolved environment variable value when the <c>env:</c> prefix is used;
    /// otherwise the original input value. Returns an empty string if the value or
    /// referenced variable is missing.
    /// </returns>
    public static string ResolveEnvValue(string value)
    {
        if (Null(value)) return string.Empty;

        value = value.Trim();
        const string prefix = "env:";
        if (!value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return value;

        string name = value.Substring(prefix.Length).Trim();
        if (Null(name)) return string.Empty;

        return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine)
            ?? Environment.GetEnvironmentVariable(name)
            ?? string.Empty;
    }

    /// <summary>
    /// Resolves the password value used for certificate or signing operations.
    /// </summary>
    /// <param name="args">
    /// The parsed command argument dictionary. The method checks the <c>pass</c> argument first
    /// and falls back to the <c>DETACHED_PFXPASS</c> environment variable when needed.
    /// </param>
    /// <returns>
    /// The resolved password value, or an empty string if no password source is available.
    /// </returns>
    public static string ResolvePassword(Dictionary<string, string> args)
    {
        string pfxPass = GetArg(args, "pass", string.Empty);
        if (Null(pfxPass))
        {
            pfxPass = Environment.GetEnvironmentVariable("DETACHED_PFXPASS", EnvironmentVariableTarget.Machine)
                ?? Environment.GetEnvironmentVariable("DETACHED_PFXPASS")
                ?? string.Empty;
        }

        return ResolveEnvValue(pfxPass);
    }
}
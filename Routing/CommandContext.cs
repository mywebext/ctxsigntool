//CtxSignTool.Routing/CommandContext.cs
using CtxSignTool.Services;
using CtxSignTool.Contracts;

namespace CtxSignTool.Routing;

/// <summary>
/// Represents the parsed command-line context for the current invocation,
/// including the raw arguments, normalized argument map, and resolved command name.
/// </summary>
public sealed class CommandContext
{
    /// <summary>
    /// Gets the raw command-line arguments exactly as they were provided to the application.
    /// </summary>
    public string[] RawArgs { get; }

    /// <summary>
    /// Gets the parsed argument dictionary for the current command invocation.
    /// Argument keys are compared using a case-insensitive comparer.
    /// </summary>
    public Dictionary<string, string> Args { get; }

    /// <summary>
    /// Gets the resolved command name used for help text and user-facing output.
    /// </summary>
    public string CommandName { get; }

    /// <summary>
    /// Gets a value indicating whether the current process is running in an interactive user environment.
    /// </summary>
    public bool IsInteractive => Environment.UserInteractive;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandContext"/> class
    /// using the provided raw arguments and parsed argument dictionary.
    /// </summary>
    /// <param name="rawArgs">
    /// The raw command-line arguments supplied to the application.
    /// If null, an empty array is used.
    /// </param>
    /// <param name="args">
    /// The parsed argument dictionary for the current invocation.
    /// If null, an empty case-insensitive dictionary is used.
    /// </param>
    public CommandContext(string[] rawArgs, Dictionary<string, string> args)
    {
        RawArgs = rawArgs ?? Array.Empty<string>();
        Args = args ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        CommandName = CommandNameService.GetCommandName();
    }

    /// <summary>
    /// Determines whether the specified argument name is present in the current context.
    /// </summary>
    /// <param name="name">
    /// The argument name to test for.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the argument exists; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Has(string name) => HasArg(Args, name);

    /// <summary>
    /// Gets the string value of the specified argument from the current context.
    /// </summary>
    /// <param name="name">
    /// The argument name to retrieve.
    /// </param>
    /// <param name="defaultValue">
    /// The value to return when the argument is not present.
    /// </param>
    /// <returns>
    /// The argument value if present; otherwise, <paramref name="defaultValue"/>.
    /// </returns>
    public string Get(string name, string defaultValue = "") => GetArg(Args, name, defaultValue);

    /// <summary>
    /// Gets the Boolean value of the specified argument from the current context.
    /// </summary>
    /// <param name="name">
    /// The argument name to retrieve.
    /// </param>
    /// <param name="defaultValue">
    /// The value to return when the argument is not present or cannot be interpreted as a Boolean value.
    /// </param>
    /// <returns>
    /// The parsed Boolean argument value if available; otherwise, <paramref name="defaultValue"/>.
    /// </returns>
    public bool GetBool(string name, bool defaultValue = false) => GetArgBool(Args, name, defaultValue);
}
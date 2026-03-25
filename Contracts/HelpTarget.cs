namespace CtxSignTool.Contracts;

/// <summary>
/// Specifies the available help targets for the application, enabling users to request help for specific features or
/// commands.
/// </summary>
/// <remarks>
/// Each member of the HelpTarget enumeration corresponds to a distinct help context, such as general
/// information, version details, or command-specific guidance. Use this enumeration to determine which help content to
/// display based on user input.
/// </remarks>
public enum HelpTarget
{
    /// <summary>
    /// No help target was specified.
    /// </summary>
    None = 0,

    /// <summary>
    /// Displays general help information and an overview of available commands.
    /// </summary>
    General = 1,

    /// <summary>
    /// Displays version information for the application and underlying libraries.
    /// </summary>
    Version = 2,

    /// <summary>
    /// Displays help information for the PrintPins command.
    /// </summary>
    PrintPins = 3,

    /// <summary>
    /// Displays help information for the MakeCert command.
    /// </summary>
    MakeCert = 4,

    /// <summary>
    /// Displays help information for the Manifest command.
    /// </summary>
    Manifest = 5,

    /// <summary>
    /// Displays help information for the Sign command.
    /// </summary>
    Sign = 6,

    /// <summary>
    /// Displays help information for the Verify command.
    /// </summary>
    Verify = 7,

    /// <summary>
    /// Displays help information for available command-line switches and parameters.
    /// </summary>
    Switches = 8
}

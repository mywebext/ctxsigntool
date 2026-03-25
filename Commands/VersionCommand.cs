using CtxSignTool.Contracts;
using CtxSignTool.Routing;
using CtxSignTool.Services;

namespace CtxSignTool.Commands;
/// <summary>
/// Provides functionality to execute a command that retrieves and displays the tool version.
/// </summary>
/// <remarks>This class is static and cannot be instantiated. It is intended for use in command-line applications
/// where the version of the tool needs to be displayed to the user.</remarks>
public static class VersionCommand
{
    /// <summary>
    /// Executes the <c>--version</c> command and prints the CtxSignTool and
    /// CtxSignLib version information to the console.
    /// </summary>
    /// <param name="context">
    /// The command context containing parsed arguments and runtime command information.
    /// </param>
    /// <returns>
    /// An integer exit code indicating the result of the operation.
    /// Returns <see cref="ReturnCodes.Ok"/> on success.
    /// </returns>
    public static int Execute(CommandContext context)
    {
        Console.WriteLine(VersionService.GetToolVersion());
        return 0;
    }
}

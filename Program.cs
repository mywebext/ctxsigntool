using CtxSignTool.Commands;
using CtxSignTool.Contracts;
using CtxSignTool.Help;
using CtxSignTool.Localization;
using CtxSignTool.Routing;
using CtxSignTool.Services;

namespace CtxSignTool;

internal class Program
{
    /// <summary>
    /// Stores the parsed command-line arguments for the current invocation,
    /// using a case-insensitive key comparer.
    /// </summary>
    /// <remarks>
    /// Keys represent argument names (for example <c>--sign</c>, <c>--verify</c>, or <c>--out</c>)
    /// and values contain the associated argument values when provided.
    /// </remarks>
    public static Dictionary<string, string> ArgMap { get; } = new(StringComparer.OrdinalIgnoreCase);

    private static int Main(string[] args)
    {
        TryParseArgs(args, out var parsedArgs);
        ArgMap.Clear();
        foreach (var kv in parsedArgs)
            ArgMap[kv.Key] = kv.Value;

        var baseDir = AppContext.BaseDirectory;
        var langPath = Path.Combine(baseDir, "Localization", "Lang", "en.json");
        if (File.Exists(langPath))
        {
            LanguageService.Load(langPath);
        }
        else
        {
            langPath = Path.Combine(baseDir, "Lang", "en.json");
            LanguageService.Load(langPath);
        }

        var context = new CommandContext(args, ArgMap);

        try
        {
            var mode = CommandDetector.Detect(context);
            return mode switch
            {
                CommandMode.Help => HelpCommand.Execute(context),
                CommandMode.Version => VersionCommand.Execute(context),
                CommandMode.PrintPins => PrintPinsCommand.Execute(context),
                CommandMode.MakeCert => MakeCertCommand.Execute(context),
                CommandMode.Manifest => ManifestCommand.Execute(context),
                CommandMode.Sign => SignCommand.Execute(context),
                CommandMode.Verify => VerifyCommand.Execute(context),
                _ => HelpCommand.Execute(context, HelpTarget.General, ReturnCodes.InvalidUsage)
            };
        }
        catch (CtxSignException ex)
        {
            if (ex.HelpTarget != HelpTarget.None)
                Console.Error.WriteLine(HelpSystem.GetHelp(ex.HelpTarget, context));

            Console.Error.WriteLine(ex.Message);
            return (int)ex.ReturnCode;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return (int)ReturnCodes.Generic;
        }
    }
}

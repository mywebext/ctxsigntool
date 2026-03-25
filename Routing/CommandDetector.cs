using CtxSignTool.Contracts;

namespace CtxSignTool.Routing;

/// <summary>
/// Provides logic for determining which command mode should be executed
/// based on the parsed command-line arguments in the current context.
/// </summary>
public static class CommandDetector
{
    /// <summary>
    /// Determines the command mode for the current invocation by inspecting
    /// the parsed arguments contained in the provided <see cref="CommandContext"/>.
    /// </summary>
    /// <param name="context">
    /// The command context containing the parsed command-line arguments.
    /// </param>
    /// <returns>
    /// A <see cref="CommandMode"/> value representing the detected command
    /// that should be executed.
    /// </returns>
    public static CommandMode Detect(CommandContext context)
    {
        if (context.RawArgs == null || context.RawArgs.Length == 0 || context.Has("help") || context.Has("h"))
            return CommandMode.Help;

        bool shortVOnly =
            context.RawArgs.Length == 1 &&
            (string.Equals(context.RawArgs[0], "--v", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(context.RawArgs[0], "-v", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(context.RawArgs[0], "v", StringComparison.OrdinalIgnoreCase));

        if (shortVOnly || context.Has("version") || context.Has("ver"))
            return CommandMode.Version;

        bool doManifest = context.Has("manifest") || context.Has("m");
        bool doSign = context.Has("sign") || context.Has("s");
        bool doVerify = context.Has("verify") || context.Has("vfy") || context.Has("check");
        bool doMakeCert = context.Has("makecert") || context.Has("mkcert") || context.Has("cert");
        bool doPrintPins = context.Has("printpins") || context.Has("pins");

        int modeCount = (doSign ? 1 : 0) + (doVerify ? 1 : 0) + (doMakeCert ? 1 : 0) + (doPrintPins ? 1 : 0) + ((!doSign && !doVerify && doManifest) ? 1 : 0);

        if (modeCount != 1)
            return CommandMode.Help;

        if (doPrintPins) return CommandMode.PrintPins;
        if (doMakeCert) return CommandMode.MakeCert;
        if (doSign) return CommandMode.Sign;
        if (doVerify) return CommandMode.Verify;
        if (doManifest) return CommandMode.Manifest;

        return CommandMode.Help;
    }
}

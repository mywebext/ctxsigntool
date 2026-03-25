using CtxSignTool.Contracts;
using CtxSignTool.Localization;
using CtxSignTool.Routing;
using CtxSignTool.Services;

namespace CtxSignTool.Help;

/// <summary>
/// Provides static methods for retrieving and displaying help information for various command targets within a
/// command-line application.
/// </summary>
/// <remarks>
/// The HelpSystem class enables context-sensitive help output, allowing users to access tailored
/// guidance for specific commands or features. Methods in this class intentionally preserve the
/// immutable CLI pin contract:
///
/// • --thumb  = signer certificate thumbprint
/// • --pin    = raw SubjectPublicKeyInfo (SPKI) public key material (PEM, base64, or hex)
/// • --pubpin = SHA-256 of SPKI (64 hex)
/// </remarks>
public static class HelpSystem
{
    /// <summary>
    /// Builds and returns the help text for the specified command or help target.
    /// </summary>
    /// <param name="target">
    /// The help target identifying which command or topic help should be generated for.
    /// </param>
    /// <param name="context">
    /// The command context containing runtime information such as the invoked command
    /// name and parsed arguments used when formatting help output.
    /// </param>
    /// <returns>
    /// A formatted help string describing the requested command or help topic.
    /// </returns>
    public static string GetHelp(HelpTarget target, CommandContext context)
    {
        if (target == HelpTarget.None || target == HelpTarget.General)
            return BuildGeneralHelp(context);

        return target switch
        {
            HelpTarget.Version => BuildVersionHelp(context),
            HelpTarget.PrintPins => BuildPrintPinsHelp(context),
            HelpTarget.MakeCert => BuildMakeCertHelp(context),
            HelpTarget.Manifest => BuildManifestHelp(context),
            HelpTarget.Sign => BuildSignHelp(context),
            HelpTarget.Verify => BuildVerifyHelp(context),
            HelpTarget.Switches => BuildSwitchHelp(context),
            _ => BuildGeneralHelp(context)
        };
    }
    /// <summary>
    /// Generates help text for the specified target and writes it to the console.
    /// </summary>
    /// <param name="target">
    /// The help target identifying which command or topic help should be displayed.
    /// </param>
    /// <param name="context">
    /// The command context containing runtime information used when formatting the help output.
    /// </param>
    /// <param name="code">
    /// The exit code that should be returned after printing the help text.
    /// Defaults to <see cref="ReturnCodes.Ok"/>.
    /// </param>
    /// <returns>
    /// An integer exit code corresponding to the provided <paramref name="code"/> value.
    /// </returns>
    public static int Print(HelpTarget target, CommandContext context, ReturnCodes code = ReturnCodes.Ok)
    {
        Console.WriteLine(GetHelp(target, context));
        return (int)code;
    }
    private static string BuildGeneralHelp(CommandContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine(LanguageService.T("app.title", "CtxSignTool"));
        sb.AppendLine();
        sb.AppendLine($"  {LanguageService.T("help.section.version", "Version")}: {VersionService.GetToolVersion()}");
        sb.AppendLine($"  Library Version: {VersionService.GetLibraryVersion()}");
        sb.AppendLine($"  Command: {context.CommandName}");
        sb.AppendLine();
        sb.AppendLine(BuildPrintPinsHelp(context));
        sb.AppendLine(BuildMakeCertHelp(context));
        sb.AppendLine(BuildManifestHelp(context));
        sb.AppendLine(BuildSignHelp(context));
        sb.AppendLine(BuildVerifyHelp(context));
        sb.AppendLine(BuildSwitchHelp(context));
        return sb.ToString().TrimEnd();
    }
    private static string BuildVersionHelp(CommandContext context)
    {
        return
            $"{LanguageService.T("app.title", "CtxSignTool")}\n" +
            $"  Version: {VersionService.GetToolVersion()}\n" +
            $"  Library Version: {VersionService.GetLibraryVersion()}\n" +
            $"  Command: {context.CommandName}";
    }
    private static string BuildPrintPinsHelp(CommandContext context)
    {
        return
            $"{LanguageService.T("help.section.printpins", "Print pins from a certificate")}:\n" +
            $"  {context.CommandName} --printpins --cert <file.cer> [--json] [--pretty] [--out <file>]\n" +
            $"  {context.CommandName} --printpins --pfx <file.pfx> [--pass <pw|env:NAME>] [--json] [--pretty] [--out <file>]\n" +
            $"    thumb  = certificate SHA-1 thumbprint\n" +
            $"    pin    = raw SubjectPublicKeyInfo (SPKI) public key material (PEM, base64, or hex)\n" +
            $"    pubpin = SHA-256 of SPKI (64 hex)\n";
    }
    private static string BuildMakeCertHelp(CommandContext context)
    {
        return
            $"{LanguageService.T("help.section.makecert", "Create a self-signed certificate")}:\n" +
            $"  {context.CommandName} --makecert --out cert.pfx --pass <password> [--cer cert.cer]\n" +
            $"         [--cn \"Common Name\"] [--days 825] [--eku code|doc|both] [--rsa 3072]\n" +
            $"         [--pinsout <file>]\n" +
            $"    Tip: --pass env:VARNAME\n";
    }
    private static string BuildManifestHelp(CommandContext context)
    {
        return
            $"{LanguageService.T("help.section.manifest", "Build or update a manifest for a directory")}:\n" +
            $"  {context.CommandName} --manifest --root <dir> [--out <cmsmanifest.json>]\n";
    }
    private static string BuildSignHelp(CommandContext context)
    {
        return
            $"{LanguageService.T("help.section.sign", "Sign a file or build+sign a manifest")}:\n" +
            $"  {context.CommandName} --sign --in <file> [--out <file.sig>] --pfx <file.pfx> [--pass <pw|env:NAME>]\n" +
            $"  {context.CommandName} --sign --in <file> [--out <file.sig>] --thumb <thumbprint> [--prompt-thumb]\n" +
            $"  {context.CommandName} --sign --manifest --root <dir> [--name <cmsmanifest.json>] [--sig <cmsmanifest.sig>] --pfx <file.pfx> [--pass <pw|env:NAME>]\n" +
            $"  {context.CommandName} --sign --manifest --root <dir> [--name <cmsmanifest.json>] [--sig <cmsmanifest.sig>] --thumb <thumbprint> [--prompt-thumb]\n";
    }
    private static string BuildVerifyHelp(CommandContext context)
    {
        return
            $"{LanguageService.T("help.section.verify", "Verify a detached signature or verify a signed manifest")}:\n" +
            $"  {context.CommandName} --verify --in <file> [--sig <file.sig>] --thumb <thumbprint>\n" +
            $"  {context.CommandName} --verify --in <file> [--sig <file.sig>] --pin <spki-pem|base64|hex>\n" +
            $"  {context.CommandName} --verify --in <file> [--sig <file.sig>] --pubpin <spki-sha256-hex>\n" +
            $"  {context.CommandName} --verify --in <file> [--sig <file.sig>] --cert <file.cer> [--pinmode pub|pin|thumb]\n" +
            $"  {context.CommandName} --verify --manifest <cmsmanifest.json> [--sig <cmsmanifest.sig>] --thumb <thumbprint>\n" +
            $"  {context.CommandName} --verify --manifest <cmsmanifest.json> [--sig <cmsmanifest.sig>] --pin <spki-pem|base64|hex>\n" +
            $"  {context.CommandName} --verify --manifest <cmsmanifest.json> [--sig <cmsmanifest.sig>] --pubpin <spki-sha256-hex>\n" +
            $"  {context.CommandName} --verify --manifest <cmsmanifest.json> --root <dir> --in <file> [--sig <cmsmanifest.sig>] --thumb <thumbprint>\n" +
            $"  {context.CommandName} --verify --manifest <cmsmanifest.json> --root <dir> --in <file> [--sig <cmsmanifest.sig>] --pin <spki-pem|base64|hex>\n" +
            $"  {context.CommandName} --verify --manifest <cmsmanifest.json> --root <dir> --in <file> [--sig <cmsmanifest.sig>] --pubpin <spki-sha256-hex>\n" +
            $"    Use exactly one of --thumb, --pin, or --pubpin.\n";
    }
    private static string BuildSwitchHelp(CommandContext context)
    {
        return
            "Switch use:\n" +
            $"  {context.CommandName} --printpins\n" +
            $"  {context.CommandName} --makecert\n" +
            $"  {context.CommandName} --manifest\n" +
            $"  {context.CommandName} --sign\n" +
            $"  {context.CommandName} --sign --manifest\n" +
            $"  {context.CommandName} --verify\n" +
            $"  {context.CommandName} --verify --manifest\n";
    }
}
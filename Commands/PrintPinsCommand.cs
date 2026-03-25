using CtxSignTool.Certificates;
using CtxSignTool.Contracts;
using CtxSignTool.Localization;
using CtxSignTool.Routing;

namespace CtxSignTool.Commands;

/// <summary>
/// Prints certificate pin information (thumb, pin, pubpin).
/// </summary>
/// <remarks>
/// Contract:
///
/// • thumb  = certificate SHA-1 thumbprint
/// • pin    = raw SubjectPublicKeyInfo (SPKI) public key material
/// • pubpin = SHA-256 of SPKI (64 hex)
///
/// The value printed for <c>pin</c> is emitted by <see cref="PinFormatter"/> as Base64
/// text of the raw SPKI bytes, but the CLI contract for <c>--pin</c> is the underlying
/// SPKI data itself, which may also be represented as PEM or hex.
/// </remarks>
public static class PrintPinsCommand
{
    /// <summary>
    /// Executes the <c>--printpins</c> command and outputs the thumbprint, raw public key pin,
    /// and SHA-256 public key pin for the specified certificate.
    /// </summary>
    /// <param name="context">
    /// The command context containing parsed arguments such as <c>--cert</c>, <c>--pfx</c>,
    /// output format options, and other command switches.
    /// </param>
    /// <returns>
    /// An integer exit code indicating the result of the operation.
    /// Returns <see cref="ReturnCodes.Ok"/> on success.
    /// </returns>
    public static int Execute(CommandContext context)
    {
        string certPath = context.Get("cert", string.Empty);
        string pfxPath = context.Get("pfx", string.Empty);

        using var cert = CertificateLoader.LoadForAny(certPath, pfxPath, context.Args);

        if (cert == null)
        {
            throw new CtxSignException(
                ReturnCodes.InvalidUsage,
                HelpTarget.PrintPins,
                LanguageService.T("error.missingcert", "Missing certificate input."));
        }

        bool json = context.Has("json");
        bool pretty = context.Has("pretty");

        if (json)
        {
            Console.WriteLine(PinFormatter.ToJson(cert, pretty));
        }
        else
        {
            Console.WriteLine(PinFormatter.ToPrettyText(cert));
        }

        return 0;
    }
}
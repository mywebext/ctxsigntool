using CtxSignTool.Certificates;
using CtxSignTool.Contracts;
using CtxSignTool.Manifest;
using CtxSignTool.Routing;
using CtxSignTool.Services;

namespace CtxSignTool.Commands;
/// <summary>
/// Provides methods for signing files or manifests using a specified signing certificate.
/// </summary>
/// <remarks>The SignCommand class enables digital signing of files or manifest directories based on the provided
/// command context. It supports both direct file signing and manifest-based signing, requiring input file paths or
/// directories and certificate details such as a PFX file or thumbprint. The class handles certificate resolution and
/// validates required parameters, throwing exceptions for missing or invalid inputs. Use this class to automate signing
/// operations in build or deployment workflows.</remarks>
public static class SignCommand
{
    /// <summary>
    /// Executes a sign operation based on the specified command context.
    /// </summary>
    /// <remarks>If the context includes the 'manifest' or 'm' flag, a manifest sign operation is executed;
    /// otherwise, a direct sign operation is performed.</remarks>
    /// <param name="context">The context that determines which sign operation to perform. Must contain relevant flags to indicate whether a
    /// manifest or direct sign is required.</param>
    /// <returns>An integer value indicating the result of the sign operation. The meaning of the value depends on the operation
    /// performed.</returns>
    public static int Execute(CommandContext context)
    {
        bool doManifest = context.Has("manifest") || context.Has("m");
        return doManifest ? ExecuteManifestSign(context) : ExecuteDirectSign(context);
    }
    private static int ExecuteDirectSign(CommandContext context)
    {
        string input = context.Get("in", string.Empty);
        if (Null(input))
            throw new CtxSignException(ReturnCodes.InvalidUsage, HelpTarget.Sign, "Missing --in <file>.");

        string outSig = context.Get("out", string.Empty);
        if (Null(outSig))
            outSig = input + ".sig";

        using var cert = ResolveSigningCertificate(context);
        CMSWriter.SignDetachment(input, outSig, cert);
        Console.WriteLine($"Signed: {input}");
        Console.WriteLine($"Wrote : {outSig}");
        return (int)ReturnCodes.Ok;
    }

    private static int ExecuteManifestSign(CommandContext context)
    {
        string root = context.Get("root", context.Get("dir", string.Empty));
        if (Null(root))
            throw new CtxSignException(ReturnCodes.InvalidUsage, HelpTarget.Manifest, "Missing --root <directory> for manifest signing.");

        string manifestPath = ManifestPathResolver.ResolveManifestPath(context, root);
        string sigPath = ManifestPathResolver.ResolveManifestSignaturePath(context, manifestPath);

        int count = ManifestBuilder.BuildOrUpdate(root, manifestPath);
        using var cert = ResolveSigningCertificate(context);
        CMSWriter.SignDetachment(manifestPath, sigPath, cert);

        Console.WriteLine($"Manifest : {manifestPath}");
        Console.WriteLine($"Files    : {count}");
        Console.WriteLine($"Signed   : {manifestPath}");
        Console.WriteLine($"Wrote    : {sigPath}");
        return (int)ReturnCodes.Ok;
    }

    private static X509Certificate2 ResolveSigningCertificate(CommandContext context)
    {
        string pfxPath = context.Get("pfx", string.Empty);
        if (!Null(pfxPath))
        {
            string pfxPass = EnvironmentValueResolver.ResolvePassword(context.Args);
            var cert = CertificateLoader.LoadPfx(pfxPath, pfxPass);
            if (!cert.HasPrivateKey)
                throw new CtxSignException(ReturnCodes.InvalidUsage, HelpTarget.Sign, "PFX certificate does not contain a private key.");
            return cert;
        }

        string thumb = context.Get("thumb", string.Empty);
        if (Null(thumb))
        {
            thumb = Environment.GetEnvironmentVariable("DETACHED_THUMBPRINT", EnvironmentVariableTarget.Machine)
                ?? Environment.GetEnvironmentVariable("DETACHED_THUMBPRINT")
                ?? string.Empty;
        }

        bool promptThumb = context.Has("prompt-thumb") || context.Has("promptthumb");
        if (Null(thumb) && promptThumb && context.IsInteractive)
        {
            Console.Write("Enter certificate thumbprint: ");
            thumb = Console.ReadLine() ?? string.Empty;
        }

        if (Null(thumb))
            throw new CtxSignException(ReturnCodes.InvalidUsage, HelpTarget.Sign, "Missing signing key. Use --pfx <file.pfx> [--pass ...] OR --thumb <thumbprint>.");

        if (!IsWindows)
            throw new CtxSignException(ReturnCodes.InvalidUsage, HelpTarget.Sign, "Thumbprint-based signing uses the Windows certificate store. On non-Windows, use --pfx.");

        thumb = NormalizeHex(thumb);
        var found = CertificateLoader.FindByThumbprint(thumb);
        if (found == null)
            throw new CtxSignException(ReturnCodes.InvalidUsage, HelpTarget.Sign, "Certificate not found for the provided thumbprint.");

        if (!found.HasPrivateKey)
            throw new CtxSignException(ReturnCodes.InvalidUsage, HelpTarget.Sign, "Found certificate, but it does not have an accessible private key.");

        return found;
    }
}

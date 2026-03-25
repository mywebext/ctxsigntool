using CtxSignTool.Certificates;
using CtxSignTool.Contracts;
using CtxSignTool.Manifest;
using CtxSignTool.Localization;
using CtxSignTool.Routing;

namespace CtxSignTool.Commands;

/// <summary>
/// Provides static methods for verifying digital signatures and manifests to ensure file integrity and authenticity.
/// </summary>
/// <remarks>
/// Contract (mirrors the immutable CLI laws in Functions.cs):
///
/// • --thumb pins by signer certificate thumbprint (compares against the signer
///   certificate embedded in the CMS signature).
/// • --pin is the signer's raw public key bytes: DER SubjectPublicKeyInfo (SPKI),
///   same bytes as PEM -----BEGIN PUBLIC KEY-----.
/// • --pubpin is the SHA-256 of --pin (64 hex).
///
/// Verification is crypto-only: SignedCms.CheckSignature(true) is used through
/// ctxsignlib. No OS trust store validation is performed, and all pin comparisons
/// are made against the signer embedded in the CMS signature.
/// </remarks>
public static class VerifyCommand
{
    /// <summary>
    /// Executes the <c>--verify</c> command and validates a detached CMS / PKCS#7 signature
    /// or signed manifest using the specified pin or certificate.
    /// </summary>
    /// <param name="context">
    /// The command context containing parsed arguments such as <c>--in</c>, <c>--sig</c>,
    /// <c>--thumb</c>, <c>--pin</c>, <c>--pubpin</c>, <c>--cert</c>, and manifest-related
    /// verification options.
    /// </param>
    /// <returns>
    /// An integer exit code indicating the result of the verification operation.
    /// Returns <see cref="ReturnCodes.Ok"/> when the signature is successfully validated.
    /// </returns>
    public static int Execute(CommandContext context)
    {
        int HelpMultiplier = (context.Has("help") || context.Has("h") || context.Has("?")) ? -1 : 0;
        bool HasManifestArgs = context.Has("manifest") || context.Has("m");
        bool HasKeyArgs = context.Has("thumb") || context.Has("pin") || context.Has("pubpin") || context.Has("cert") || context.Has("pfx");
        bool CanDeriveKeySignatureArgs = HasKeyArgs && context.Has("sig") || context.Has("manifest") || context.Has("m");

        bool CanVerify = HasKeyArgs && context.Has("sig") || context.Has("name") || context.Has("manifest") || context.Has("m");
        //Thruput becomes the multiplier that pulls the Verify command out of Help mode
        int Thruput = 1 * (context.Has("thumb") || context.Has("pin") || context.Has("pubpin") || context.Has("cert") || context.Has("pfx") ? 1 : 0);
        Thruput += (context.Has("sig") || context.Has("name")) ? 1 : 0;
        bool hasManifest = context.Has("manifest") || context.Has("m");
        bool hasInput = !Null(context.Get("in", string.Empty));
        bool hasSig = !Null(context.Get("sig", string.Empty));
        bool hasRoot = !Null(context.Get("root", context.Get("dir", string.Empty)));
        bool isPartial = context.Has("partial") || context.Has("p");
        bool isDetailed = context.Has("detailed") || context.Has("detail") || context.Has("d");

        bool hasThumb = !Null(context.Get("thumb", string.Empty));
        bool hasPin = !Null(context.Get("pin", string.Empty));
        bool hasPubPin = !Null(context.Get("pubpin", string.Empty));
        bool hasCert = !Null(context.Get("cert", string.Empty));
        bool hasPfx = !Null(context.Get("pfx", string.Empty));
        bool hasAnyPin = hasThumb || hasPin || hasPubPin || hasCert || hasPfx;

        bool directVerify = !hasManifest;
        bool manifestVerify = hasManifest;

        bool manifestNeedsSigError = manifestVerify && !hasSig;          // E
        bool manifestStrict = manifestVerify && hasSig && !isPartial;    // F, H, L, N
        bool manifestPartial = manifestVerify && hasSig && isPartial;    // G, I, M
        bool manifestSingleFile = manifestVerify && hasSig && hasInput;  // J, K, O
        bool manifestFullSet = manifestVerify && hasSig && !hasInput;    // F, G, H, I, L, M, N
        bool customRoot = manifestVerify && hasRoot;                     // L, M, N, O
        bool detailedOutput = manifestVerify && isDetailed;              // H, I, K, N

        if (manifestNeedsSigError)
        {
            throw new CtxSignException(
                ReturnCodes.InvalidUsage,
                HelpTarget.Verify,
                LanguageService.T("error.verify.missingsig", "Missing required parameter: --sig"));
        }

        if (directVerify)
            return ExecuteDirectVerify(context);

        return ExecuteManifestVerify(
            context,
            manifestStrict,
            manifestPartial,
            manifestSingleFile,
            manifestFullSet,
            customRoot,
            detailedOutput,
            hasAnyPin);
    }
    private enum VerifyThruput
    {
        VerifyMaifestAutopfx,
        VerifyMaifestAutoThumb,
        VerifyNaifestAutoPin,
        VerifyNaifestName,
        VerifyManifestSig,
        ManifestStrict,
        ManifestPartial,
        ManifestSingleFile,
        ManifestFullSet
    }
    private static int ExecuteDirectVerify(CommandContext context)
    {
        string input = context.Get("in", string.Empty);
        if (Null(input))
        {
            throw new CtxSignException(
                ReturnCodes.InvalidUsage,
                HelpTarget.Verify,
                LanguageService.T("error.missinginput", "Missing --in <file>."));
        }

        string sigPath = context.Get("sig", string.Empty);
        if (Null(sigPath))
            sigPath = input + ".sig";

        var pins = ResolvePins(context);

        VerifyResult vr =
            !Null(pins.PubPin) ? CMSVerifier.VerifyDetachmentByPublicKey(input, sigPath, NormalizeHex(pins.PubPin)) :
            !Null(pins.Pin) ? CMSVerifier.VerifyDetachmentByRawPublicKey(input, sigPath, pins.Pin) :
                              CMSVerifier.VerifyDetachmentByThumbprint(input, sigPath, NormalizeHex(pins.Thumb));

        if (vr == VerifyResult.Ok)
        {
            Console.WriteLine(LanguageService.T("ok", "OK"));
            return 0;
        }

        Console.Error.WriteLine(vr.ToString());
        return (int)vr;
    }

    private static int ExecuteManifestVerify(
        CommandContext context,
        bool manifestStrict,
        bool manifestPartial,
        bool manifestSingleFile,
        bool manifestFullSet,
        bool customRoot,
        bool detailedOutput,
        bool hasAnyPin)
    {
        _ = manifestStrict;
        _ = customRoot;
        _ = hasAnyPin;

        string rootArg = context.Get("root", context.Get("dir", string.Empty));
        string manifestPath = ResolveVerifyManifestPath(context, rootArg);

        string sigPath = context.Get("sig", string.Empty);
        if (Null(sigPath))
        {
            throw new CtxSignException(
                ReturnCodes.InvalidUsage,
                HelpTarget.Verify,
                LanguageService.T("error.verify.missingsig", "Missing required parameter: --sig"));
        }

        string effectiveRoot = !Null(rootArg)
            ? Path.GetFullPath(rootArg)
            : Path.GetDirectoryName(Path.GetFullPath(manifestPath)) ?? Directory.GetCurrentDirectory();

        var pins = ResolvePins(context);

        VerifyResult VerifyManifestSignature()
        {
            return !Null(pins.PubPin) ? CMSVerifier.VerifyDetachmentByPublicKey(manifestPath, sigPath, NormalizeHex(pins.PubPin)) :
                   !Null(pins.Pin) ? CMSVerifier.VerifyDetachmentByRawPublicKey(manifestPath, sigPath, pins.Pin) :
                                     CMSVerifier.VerifyDetachmentByThumbprint(manifestPath, sigPath, NormalizeHex(pins.Thumb));
        }

        static void WriteDetailedResult(ManifestPartialVerificationResult result)
        {
            Console.WriteLine($"Passed: {result.PassedFiles.Count}");
            Console.WriteLine($"Missing: {result.MissingFiles.Count}");
            Console.WriteLine($"Failed: {result.FailedFiles.Count}");
            Console.WriteLine($"Unreadable: {result.UnreadableFiles.Count}");

            if (result.FailedFiles.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("Failed files:");
                foreach (var file in result.FailedFiles)
                    Console.WriteLine($"  {file}");
            }

            if (result.MissingFiles.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("Missing files:");
                foreach (var file in result.MissingFiles)
                    Console.WriteLine($"  {file}");
            }

            if (result.UnreadableFiles.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("Unreadable files:");
                foreach (var file in result.UnreadableFiles)
                    Console.WriteLine($"  {file}");
            }
        }

        string input = context.Get("in", string.Empty);

        if (manifestSingleFile)
        {
            bool ok;
            VerifyResult signatureResult;
            string expectedSha256;
            string actualSha256;
            string failure;

            if (!Null(pins.PubPin))
            {
                ok = ManifestFileVerification.VerifyFileAgainstSignedManifestByPublicKey(
                    effectiveRoot,
                    input,
                    manifestPath,
                    sigPath,
                    NormalizeHex(pins.PubPin),
                    out signatureResult,
                    out expectedSha256,
                    out actualSha256,
                    out failure);
            }
            else if (!Null(pins.Thumb))
            {
                ok = ManifestFileVerification.VerifyFileAgainstSignedManifest(
                    effectiveRoot,
                    input,
                    manifestPath,
                    sigPath,
                    NormalizeHex(pins.Thumb),
                    out signatureResult,
                    out expectedSha256,
                    out actualSha256,
                    out failure);
            }
            else
            {
                signatureResult = VerifyManifestSignature();
                if (signatureResult != VerifyResult.Ok)
                {
                    Console.Error.WriteLine(signatureResult.ToString());
                    return (int)signatureResult;
                }

                string targetPath = Path.IsPathRooted(input) ? input : Path.Combine(effectiveRoot, input);
                targetPath = Path.GetFullPath(targetPath);

                expectedSha256 = string.Empty;
                actualSha256 = string.Empty;
                failure = string.Empty;

                if (!File.Exists(targetPath))
                {
                    failure = "FileMissing";
                    ok = false;
                }
                else if (!ManifestInspector.TryFindHash(manifestPath, targetPath, effectiveRoot, out expectedSha256))
                {
                    failure = "FileNotInManifest";
                    ok = false;
                }
                else
                {
                    actualSha256 = FileSha256(targetPath);
                    ok = HexEquals(expectedSha256, actualSha256);
                    if (!ok)
                        failure = "HashMismatch";
                }
            }

            if (ok)
            {
                Console.WriteLine(LanguageService.T("ok", "OK"));
                return 0;
            }

            if (signatureResult != VerifyResult.Ok)
            {
                Console.Error.WriteLine(signatureResult.ToString());
                return (int)signatureResult;
            }

            if (detailedOutput)
            {
                if (!Null(expectedSha256))
                    Console.Error.WriteLine($"Expected: {expectedSha256}");
                if (!Null(actualSha256))
                    Console.Error.WriteLine($"Actual:   {actualSha256}");
            }

            Console.Error.WriteLine(
                Null(failure)
                    ? LanguageService.T("error.verify.failed", "Signature verification failed")
                    : failure);

            return (int)ReturnCodes.BadSignature;
        }

        if (manifestFullSet)
        {
            if (detailedOutput)
            {
                if (!Null(pins.PubPin) && manifestPartial)
                {
                    VerifyResult signatureResult;
                    var result = SignedManifestPartialVerifier.VerifySignedManifestPartialDetailed(
                        effectiveRoot,
                        manifestPath,
                        sigPath,
                        NormalizeHex(pins.PubPin),
                        out signatureResult);

                    if (signatureResult != VerifyResult.Ok)
                    {
                        Console.Error.WriteLine(signatureResult.ToString());
                        return (int)signatureResult;
                    }

                    WriteDetailedResult(result);
                    if (result.Success)
                    {
                        Console.WriteLine(LanguageService.T("ok", "OK"));
                        return 0;
                    }

                    return (int)ReturnCodes.BadSignature;
                }

                var manifestResult = VerifyManifestSignature();
                if (manifestResult != VerifyResult.Ok)
                {
                    Console.Error.WriteLine(manifestResult.ToString());
                    return (int)manifestResult;
                }

                var detailedResult = manifestPartial
                    ? ManifestPartialVerifier.VerifyManifestPartialDetailed(effectiveRoot, manifestPath)
                    : DetailedManifestFileVerification.VerifyManifestDetailedResult(effectiveRoot, manifestPath);

                WriteDetailedResult(detailedResult);
                if (detailedResult.Success)
                {
                    Console.WriteLine(LanguageService.T("ok", "OK"));
                    return 0;
                }

                return (int)ReturnCodes.BadSignature;
            }

            if (!Null(pins.PubPin))
            {
                if (manifestPartial)
                {
                    VerifyResult signatureResult;
                    bool ok = SignedManifestPartialVerifier.VerifySignedManifestPartial(
                        effectiveRoot,
                        manifestPath,
                        sigPath,
                        NormalizeHex(pins.PubPin),
                        out signatureResult);

                    if (!ok)
                    {
                        if (signatureResult != VerifyResult.Ok)
                        {
                            Console.Error.WriteLine(signatureResult.ToString());
                            return (int)signatureResult;
                        }

                        Console.Error.WriteLine(LanguageService.T("error.verify.failed", "Signature verification failed"));
                        return (int)ReturnCodes.BadSignature;
                    }

                    Console.WriteLine(LanguageService.T("ok", "OK"));
                    return 0;
                }

                {
                    VerifyResult signatureResult;
                    Dictionary<string, List<string>> failedFiles;
                    bool ok = SignedManifestVerifier.VerifySignedManifest(
                        effectiveRoot,
                        manifestPath,
                        sigPath,
                        NormalizeHex(pins.PubPin),
                        out signatureResult,
                        out failedFiles);

                    if (!ok)
                    {
                        if (signatureResult != VerifyResult.Ok)
                        {
                            Console.Error.WriteLine(signatureResult.ToString());
                            return (int)signatureResult;
                        }

                        Console.Error.WriteLine(LanguageService.T("error.verify.failed", "Signature verification failed"));
                        return (int)ReturnCodes.BadSignature;
                    }

                    Console.WriteLine(LanguageService.T("ok", "OK"));
                    return 0;
                }
            }

            {
                var manifestResult = VerifyManifestSignature();
                if (manifestResult != VerifyResult.Ok)
                {
                    Console.Error.WriteLine(manifestResult.ToString());
                    return (int)manifestResult;
                }

                bool ok = manifestPartial
                    ? ManifestPartialVerifier.VerifyManifestPartial(effectiveRoot, manifestPath)
                    : ManifestVerifier.VerifyManifest(effectiveRoot, manifestPath, out _);

                if (ok)
                {
                    Console.WriteLine(LanguageService.T("ok", "OK"));
                    return 0;
                }

                Console.Error.WriteLine(LanguageService.T("error.verify.failed", "Signature verification failed"));
                return (int)ReturnCodes.BadSignature;
            }
        }

        throw new CtxSignException(
            ReturnCodes.InvalidUsage,
            HelpTarget.Verify,
            LanguageService.T("error.verify.invalidstate", "Invalid verify routing state."));
    }

    private static string ResolveVerifyManifestPath(CommandContext context, string root)
    {
        string manifestArg = context.Get("manifest", string.Empty);
        if (!Null(manifestArg) && !string.Equals(manifestArg, "true", StringComparison.OrdinalIgnoreCase))
            return manifestArg;

        string input = context.Get("in", string.Empty);
        if (!Null(input) && string.Equals(Path.GetExtension(input), ".json", StringComparison.OrdinalIgnoreCase))
            return input;

        if (!Null(root))
            return Path.Combine(root, "cmsmanifest.json");

        throw new CtxSignException(
            ReturnCodes.InvalidUsage,
            HelpTarget.Verify,
            "Missing manifest path. Use --manifest <cmsmanifest.json> or provide --root <dir>.");
    }

    private static VerifyPins ResolvePins(CommandContext context)
    {
        string thumb = context.Get("thumb", string.Empty);
        string pin = context.Get("pin", string.Empty);
        string pubpin = context.Get("pubpin", string.Empty);

        int supplied =
            (Null(thumb) ? 0 : 1) +
            (Null(pin) ? 0 : 1) +
            (Null(pubpin) ? 0 : 1);

        if (supplied > 1)
        {
            throw new CtxSignException(
                ReturnCodes.InvalidUsage,
                HelpTarget.Verify,
                "Use exactly one of --thumb, --pin, or --pubpin.");
        }

        if (supplied == 0)
        {
            string certPath = context.Get("cert", string.Empty);
            string pfxPath = context.Get("pfx", string.Empty);

            string pinMode = (context.Get("pinmode", "pub") ?? "pub").Trim().ToLowerInvariant();
            if (pinMode != "pub" && pinMode != "pin" && pinMode != "thumb")
                pinMode = "pub";

            using var cert = CertificateLoader.LoadForAny(certPath, pfxPath, context.Args);
            if (cert != null)
            {
                if (pinMode == "thumb")
                    thumb = PinFormatter.GetThumb(cert);
                else if (pinMode == "pin")
                    pin = PinFormatter.GetPin(cert);
                else
                    pubpin = PinFormatter.GetPubPin(cert);
            }
        }

        if (Null(thumb) && Null(pin) && Null(pubpin))
        {
            throw new CtxSignException(
                ReturnCodes.InvalidUsage,
                HelpTarget.Verify,
                LanguageService.T("error.verify.missingpin", "Missing pin."));
        }

        return new VerifyPins(thumb, pin, pubpin);
    }

    private static bool SamePath(string a, string b)
    {
        if (Null(a) || Null(b))
            return false;

        return string.Equals(
            Path.GetFullPath(a),
            Path.GetFullPath(b),
            IsWindows ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }

    private readonly record struct VerifyPins(string Thumb, string Pin, string PubPin);
}
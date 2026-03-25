using CtxSignTool.Certificates;
using CtxSignTool.Contracts;
using CtxSignTool.Output;
using CtxSignTool.Routing;
using CtxSignTool.Services;

namespace CtxSignTool.Commands;
/// <summary>
/// Provides functionality to generate a self-signed certificate using parameters supplied via a command context.
/// </summary>
/// <remarks>This class offers a command for creating self-signed certificates, applying default values and
/// enforcing constraints such as minimum RSA key size and valid extended key usage modes. The generated certificate and
/// related information are output to the console, and optional files are written as specified. Use this class when you
/// need to automate certificate creation for development or testing scenarios.</remarks>
public static class MakeCertCommand
{
    /// <summary>
    /// Executes the command to generate a self-signed certificate using the specified command context parameters.
    /// </summary>
    /// <remarks>This method validates input parameters and creates a self-signed certificate, outputting
    /// relevant information to the console. Default values are applied when parameters are not specified, and
    /// constraints such as minimum RSA key size and valid extended key usage modes are enforced.</remarks>
    /// <param name="context">The command context containing parameters for certificate generation, including output file names, common name,
    /// validity period, RSA key size, extended key usage, and additional options.</param>
    /// <returns>An integer status code indicating the result of the execution. A value of 0 indicates success.</returns>
    public static int Execute(CommandContext context)
    {
        string outPfx = context.Get("out", "selfsigned.pfx");
        string outCer = context.Get("cer", string.Empty);
        string cn = context.Get("cn", "CtxSign Self-Signed");
        if (Null(cn)) cn = "CtxSign Self-Signed";

        int days = int.TryParse(context.Get("days", "825"), out var parsedDays) && parsedDays > 0 ? parsedDays : 825;
        int rsaBits = int.TryParse(context.Get("rsa", "3072"), out var parsedBits) && parsedBits >= 2048 ? parsedBits : 3072;
        string ekuMode = (context.Get("eku", "code") ?? "code").Trim().ToLowerInvariant();
        if (ekuMode != "code" && ekuMode != "doc" && ekuMode != "both")
            ekuMode = "code";

        string pfxPass = EnvironmentValueResolver.ResolvePassword(context.Args);

        CreateSelfSignedCert(cn, days, rsaBits, ekuMode, outPfx, pfxPass, outCer);

        Console.WriteLine("OK");
        Console.WriteLine($"PFX  : {outPfx}");
        if (!Null(outCer)) Console.WriteLine($"CER  : {outCer}");
        Console.WriteLine($"CN   : {cn}");
        Console.WriteLine($"Days : {days}");
        Console.WriteLine($"EKU  : {ekuMode}");
        Console.WriteLine();

        using var cert = !Null(outCer)
            ? CertificateLoader.LoadCer(outCer)
            : CertificateLoader.LoadPfx(outPfx, pfxPass);

        string prettyPins = PinFormatter.ToPrettyText(cert);
        Console.WriteLine(prettyPins);

        string pinsOut = context.Get("pinsout", string.Empty);
        if (!Null(pinsOut))
            FileOutput.MaybeWrite(pinsOut, prettyPins + Environment.NewLine);

        return (int)ReturnCodes.Ok;
    }

    private static void CreateSelfSignedCert(string commonName, int daysValid, int rsaKeySize, string ekuMode, string outPfxPath, string pfxPassword, string outCerPath)
    {
        if (Null(outPfxPath))
            throw new CtxSignException(ReturnCodes.InvalidUsage, HelpTarget.MakeCert, "Missing --out <file.pfx>.");

        var subject = new X500DistinguishedName($"CN={commonName}");
        using RSA rsa = RSA.Create(rsaKeySize);
        var req = new CertificateRequest(subject, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        req.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
        req.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.NonRepudiation, false));

        var eku = new OidCollection();
        if (ekuMode == "code" || ekuMode == "both") eku.Add(new Oid("1.3.6.1.5.5.7.3.3"));
        if (ekuMode == "doc" || ekuMode == "both") eku.Add(new Oid("1.3.6.1.4.1.311.10.3.12"));
        if (eku.Count > 0) req.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(eku, false));

        req.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(req.PublicKey, false));

        DateTimeOffset notBefore = DateTimeOffset.UtcNow.AddMinutes(-5);
        DateTimeOffset notAfter = notBefore.AddDays(daysValid);

        using var cert = req.CreateSelfSigned(notBefore, notAfter);
        WriteAllBytesAtomic(outPfxPath, cert.Export(X509ContentType.Pfx, pfxPassword ?? string.Empty));
        if (!Null(outCerPath))
            WriteAllBytesAtomic(outCerPath, cert.Export(X509ContentType.Cert));
    }
}

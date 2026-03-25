using CtxSignTool.Localization;

namespace CtxSignTool.Certificates;

/// <summary>
/// Provides static methods for formatting and retrieving certificate pin values
/// used by ctxsigntool verification flows.
/// </summary>
/// <remarks>
/// Contract:
///
/// • thumb  = signer certificate thumbprint
/// • pin    = raw SubjectPublicKeyInfo (SPKI) public key material
/// • pubpin = SHA-256 of SPKI, normalized as 64 hex
///
/// The value emitted by <see cref="GetPin(X509Certificate2)"/> is Base64 text of the
/// raw SPKI bytes, but the CLI contract for <c>--pin</c> is the underlying SPKI data
/// itself, which may also be represented as PEM or hex.
/// </remarks>
public static class PinFormatter
{
    /// <summary>
    /// Gets the normalized thumbprint of the specified X.509 certificate.
    /// </summary>
    public static string GetThumb(X509Certificate2 cert) =>
        NormalizeHex(cert.Thumbprint ?? string.Empty);

    /// <summary>
    /// Gets the SHA-256 public pin of the certificate's SPKI.
    /// </summary>
    public static string GetPubPin(X509Certificate2 cert) =>
        NormalizeHex(PublicKeySha256(cert));

    /// <summary>
    /// Gets the certificate pin as Base64 text of the raw DER SubjectPublicKeyInfo (SPKI) bytes.
    /// </summary>
    /// <remarks>
    /// This is the same key material represented by PEM:
    /// -----BEGIN PUBLIC KEY-----
    /// </remarks>
    public static string GetPin(X509Certificate2 cert)
    {
        byte[] spki = GetSpkiBytes(cert);
        return spki.Length == 0 ? string.Empty : Convert.ToBase64String(spki);
    }

    /// <summary>
    /// Formats the certificate details into a human-readable string.
    /// </summary>
    public static string ToPrettyText(X509Certificate2 cert)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{LanguageService.T("label.subject", "Subject")}   : {cert.Subject}");
        sb.AppendLine($"{LanguageService.T("label.notbefore", "NotBefore")} : {cert.NotBefore:u}");
        sb.AppendLine($"{LanguageService.T("label.notafter", "NotAfter")}  : {cert.NotAfter:u}");
        sb.AppendLine($"{LanguageService.T("label.thumb", "Thumb")}     : {GetThumb(cert)}");
        sb.AppendLine($"{LanguageService.T("label.pin", "Pin(SPKI)")}   : {GetPin(cert)}");
        sb.AppendLine($"{LanguageService.T("label.pubpin", "PubPin")}    : {GetPubPin(cert)}");
        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Converts the certificate details to JSON.
    /// </summary>
    public static string ToJson(X509Certificate2 cert, bool pretty)
    {
        var obj = new
        {
            subject = cert.Subject ?? string.Empty,
            notBeforeUtc = cert.NotBefore.ToUniversalTime().ToString("O"),
            notAfterUtc = cert.NotAfter.ToUniversalTime().ToString("O"),
            thumb = GetThumb(cert),
            pin = GetPin(cert),
            pubpin = GetPubPin(cert)
        };

        return JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            WriteIndented = pretty
        });
    }
}
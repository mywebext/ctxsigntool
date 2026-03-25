using CtxSignTool.Services;
namespace CtxSignTool.Certificates;
/// <summary>
/// Provides methods for loading X.509 certificates from various sources, including PFX and CER files, as well as from
/// the certificate store using thumbprints.
/// </summary>
/// <remarks>This class facilitates the retrieval of certificates for secure communications and authentication,
/// handling both file-based and store-based certificate loading.</remarks>
public static class CertificateLoader
{
    /// <summary>
    /// Loads an X.509 certificate from a specified PFX file using the provided password.
    /// </summary>
    /// <remarks>The method attempts to load the certificate with an ephemeral key set. If the loading fails,
    /// it retries without the ephemeral key set.</remarks>
    /// <param name="pfxPath">The path to the PFX file that contains the certificate. This parameter cannot be null or empty.</param>
    /// <param name="password">The password used to access the PFX file. If not provided, an empty string is used.</param>
    /// <returns>An instance of <see cref="X509Certificate2"/> representing the loaded certificate.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="pfxPath"/> is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown if the specified PFX file does not exist at the given path.</exception>
    public static X509Certificate2 LoadPfx(string pfxPath, string password)
    {
        if (Null(pfxPath))
            throw new ArgumentException("Missing --pfx <file.pfx>.");

        if (!File.Exists(pfxPath))
            throw new FileNotFoundException("PFX file not found.", pfxPath);

        var flags = X509KeyStorageFlags.EphemeralKeySet;
        try
        {
            return new X509Certificate2(pfxPath, password ?? string.Empty, flags);
        }
        catch
        {
            return new X509Certificate2(pfxPath, password ?? string.Empty);
        }
    }
    /// <summary>
    /// Loads an X.509 certificate from the specified file path.
    /// </summary>
    /// <remarks>Ensure that the file path points to a valid .cer file to avoid exceptions.</remarks>
    /// <param name="certPath">The path to the certificate file. This parameter cannot be null.</param>
    /// <returns>An instance of <see cref="X509Certificate2"/> representing the loaded certificate.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="certPath"/> is null.</exception>
    /// <exception cref="FileNotFoundException">Thrown if the certificate file does not exist at the specified path.</exception>
    public static X509Certificate2 LoadCer(string certPath)
    {
        if (Null(certPath))
            throw new ArgumentException("Missing --cert <file.cer>.");

        if (!File.Exists(certPath))
            throw new FileNotFoundException("Certificate file not found.", certPath);

        return new X509Certificate2(certPath);
    }
    /// <summary>
    /// Loads an X.509 certificate from either a certificate file or a PFX file, depending on which path is provided.
    /// </summary>
    /// <remarks>If both paths are provided, the method prioritizes loading from the certificate file. The
    /// password for the PFX file is resolved from the provided arguments.</remarks>
    /// <param name="certPath">The file path to the certificate (.cer) to load. Cannot be null.</param>
    /// <param name="pfxPath">The file path to the PFX file to load. Cannot be null.</param>
    /// <param name="args">A dictionary containing additional arguments, such as the password for the PFX file.</param>
    /// <returns>An X509Certificate2 object representing the loaded certificate, or null if neither path is provided.</returns>
    public static X509Certificate2? LoadForAny(string certPath, string pfxPath, Dictionary<string, string> args)
    {
        if (!Null(certPath))
            return LoadCer(certPath);

        if (!Null(pfxPath))
        {
            string pfxPass = EnvironmentValueResolver.ResolvePassword(args);
            return LoadPfx(pfxPath, pfxPass);
        }

        return null;
    }
    /// <summary>
    /// Finds an X.509 certificate in the current user's or local machine's certificate store using the specified
    /// thumbprint.
    /// </summary>
    /// <remarks>This method checks both the current user's and local machine's certificate stores for the
    /// specified thumbprint. It returns null if the platform is not Windows or if the thumbprint is null.</remarks>
    /// <param name="thumbHex">The hexadecimal representation of the certificate thumbprint to search for. Must not be null.</param>
    /// <returns>An X509Certificate2 object representing the found certificate, or null if no certificate is found or if the
    /// operation is not supported on the current platform.</returns>
    public static X509Certificate2? FindByThumbprint(string thumbHex)
    {
        if (!IsWindows || Null(thumbHex))
            return null;

        return FindInStore(StoreLocation.CurrentUser, thumbHex)
            ?? FindInStore(StoreLocation.LocalMachine, thumbHex);
    }

    private static X509Certificate2? FindInStore(StoreLocation loc, string thumbHex)
    {
        using var store = new X509Store(StoreName.My, loc);
        store.Open(OpenFlags.ReadOnly);

        foreach (var c in store.Certificates)
        {
            string t = NormalizeHex(c.Thumbprint ?? string.Empty);
            if (HexEquals(t, thumbHex))
                return new X509Certificate2(c);
        }

        return null;
    }
}

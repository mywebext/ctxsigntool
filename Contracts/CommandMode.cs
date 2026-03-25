namespace CtxSignTool.Contracts;

/// <summary>
/// Specifies the available command modes for the application, enabling selection
/// of the operation to execute for the current command-line invocation.
/// </summary>
/// <remarks>
/// Each value represents a distinct command that can be routed and executed by the application.
/// The <see cref="None"/> value indicates that no command mode was detected.
/// </remarks>
public enum CommandMode
{
    /// <summary>
    /// No command mode was detected.
    /// </summary>
    None = 0,

    /// <summary>
    /// Displays general or command-specific help information.
    /// </summary>
    Help,

    /// <summary>
    /// Displays version information for CtxSignTool and the underlying library.
    /// </summary>
    Version,

    /// <summary>
    /// Prints certificate thumbprint, raw public key pin, and SHA-256 public key pin values.
    /// </summary>
    PrintPins,

    /// <summary>
    /// Creates a self-signed certificate for development or testing workflows.
    /// </summary>
    MakeCert,

    /// <summary>
    /// Builds or updates a manifest for a directory of files.
    /// </summary>
    Manifest,

    /// <summary>
    /// Signs a file or manifest using a certificate or private key source.
    /// </summary>
    Sign,

    /// <summary>
    /// Verifies a detached signature or signed manifest using a thumbprint, pin, pubpin, or certificate.
    /// </summary>
    Verify
}

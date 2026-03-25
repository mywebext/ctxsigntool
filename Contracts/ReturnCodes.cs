namespace CtxSignTool.Contracts;

/// <summary>
/// Defines the set of return codes that indicate the outcome of an operation.
/// </summary>
/// <remarks>
/// Each return code represents a specific result or error condition that may occur during processing.
/// Use these codes to determine whether an operation succeeded or failed, and to identify the reason
/// for failure when applicable.
/// </remarks>
public enum ReturnCodes
{
    /// <summary>
    /// The operation completed successfully.
    /// </summary>
    Ok = 0,

    /// <summary>
    /// A general or unspecified error occurred.
    /// </summary>
    Generic = 1,

    /// <summary>
    /// The command-line arguments were invalid or incomplete.
    /// </summary>
    InvalidUsage = 2,

    /// <summary>
    /// A required detached signature file was not found.
    /// </summary>
    SignatureMissing = 3,

    /// <summary>
    /// The signature verification failed because the signature is invalid.
    /// </summary>
    BadSignature = 10,

    /// <summary>
    /// No signer certificate was found in the signature.
    /// </summary>
    NoSigner = 11,

    /// <summary>
    /// The signature was created by a signer that does not match the expected thumbprint, pin, or pubpin.
    /// </summary>
    WrongSigner = 12,

    /// <summary>
    /// A manifest generation or manifest verification operation failed.
    /// </summary>
    ManifestFailure = 20,

    /// <summary>
    /// A certificate loading, parsing, or validation error occurred.
    /// </summary>
    CertificateFailure = 30
}
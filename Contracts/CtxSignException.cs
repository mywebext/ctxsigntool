using CtxSignTool.Contracts;

namespace CtxSignTool.Contracts;
/// <summary>
/// Represents an error that occurs during a context signing operation.
/// </summary>
/// <remarks>This exception provides a return code indicating the specific error and a help target to assist in
/// resolving the issue. It is intended to be thrown when a context signing process fails, allowing callers to handle
/// the error and access additional guidance as needed.</remarks>
public sealed class CtxSignException : Exception
{
    /// <summary>
    /// Gets the return code that indicates the result of the operation.
    /// </summary>
    /// <remarks>The return code provides information about the success or failure of the operation. It can be
    /// used to determine the next steps in error handling or processing based on the operation's outcome.</remarks>
    public ReturnCodes ReturnCode { get; }
    /// <summary>
    /// Gets the help target associated with this instance.
    /// </summary>
    /// <remarks>Use this property to retrieve contextual help information relevant to the current object or
    /// operation. The returned value can be used to access documentation or guidance specific to the
    /// instance.</remarks>
    public HelpTarget HelpTarget { get; }
    /// <summary>
    /// Initializes a new instance of the CtxSignException class with a specified error code, help target, and error
    /// message.
    /// </summary>
    /// <remarks>This constructor is typically used to create exceptions that provide detailed information
    /// about context signing failures, including error codes and guidance for resolution.</remarks>
    /// <param name="returnCode">The error code that identifies the specific context signing error condition.</param>
    /// <param name="helpTarget">The help target that provides guidance or documentation relevant to resolving the exception.</param>
    /// <param name="message">The message that describes the error and provides additional context for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception, or null if no inner exception is specified.</param>
    public CtxSignException(ReturnCodes returnCode, HelpTarget helpTarget, string message, Exception? inner = null)
        : base(message, inner)
    {
        ReturnCode = returnCode;
        HelpTarget = helpTarget;
    }
}

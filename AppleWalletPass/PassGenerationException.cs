namespace SPC.Infrastructure.AppleWalletPass;

/// <summary>
/// Represents wallet pass generation failure categories.
/// </summary>
public enum PassGenerationErrorCode
{
    /// <summary>
    /// The configured certificate file could not be found.
    /// </summary>
    CertificateNotFound,

    /// <summary>
    /// The configured certificate is expired or not currently valid.
    /// </summary>
    CertificateExpired,

    /// <summary>
    /// PKCS#7 signature creation failed.
    /// </summary>
    SigningFailed,

    /// <summary>
    /// Pass data failed validation.
    /// </summary>
    InvalidPassData,

    /// <summary>
    /// A required image asset is missing.
    /// </summary>
    ImageMissing
}

/// <summary>
/// Represents a pass generation error with a machine-readable code.
/// </summary>
public sealed class PassGenerationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PassGenerationException"/> class.
    /// </summary>
    /// <param name="errorCode">The wallet pass error code.</param>
    /// <param name="message">The exception message.</param>
    public PassGenerationException(PassGenerationErrorCode errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PassGenerationException"/> class.
    /// </summary>
    /// <param name="errorCode">The wallet pass error code.</param>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The underlying exception.</param>
    public PassGenerationException(PassGenerationErrorCode errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Gets the wallet pass error code.
    /// </summary>
    public PassGenerationErrorCode ErrorCode { get; }
}

namespace AppleWalletPass;

/// <summary>
/// Identifies a Wallet pass asset slot used by the designer and packaging flow.
/// </summary>
public enum DesignerAssetSlot
{
    /// <summary>
    /// The small icon shown by Wallet in list and notification surfaces.
    /// </summary>
    Icon,

    /// <summary>
    /// The logo displayed on the front of the pass when provided.
    /// </summary>
    Logo,

    /// <summary>
    /// The optional background image displayed behind the pass content.
    /// </summary>
    Background
}

/// <summary>
/// Represents the metadata returned after an asset upload succeeds.
/// </summary>
public sealed class AssetUploadResponse
{
    /// <summary>
    /// Gets the opaque server-side token for the stored asset.
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// Gets the original file name supplied during upload.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Gets the preview URL used by the UI to display the uploaded asset.
    /// </summary>
    public required string PreviewUrl { get; init; }

    /// <summary>
    /// Gets the asset content type.
    /// </summary>
    public required string ContentType { get; init; }
}

/// <summary>
/// Represents a stored asset record resolved from server-managed storage.
/// </summary>
public sealed class AssetFileRecord
{
    /// <summary>
    /// Gets the opaque server-side token for the stored asset.
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// Gets the absolute file path of the stored asset.
    /// </summary>
    public required string FilePath { get; init; }

    /// <summary>
    /// Gets the original file name supplied during upload.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Gets the asset content type.
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    /// Gets the Wallet asset slot represented by this record.
    /// </summary>
    public required DesignerAssetSlot Slot { get; init; }
}

/// <summary>
/// Defines server-managed storage operations for uploaded designer assets.
/// </summary>
public interface IDesignerAssetStore
{
    /// <summary>
    /// Saves an uploaded asset and returns metadata required by the UI.
    /// </summary>
    Task<AssetUploadResponse> SaveAsync(
        DesignerAssetSlot slot,
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken);

    /// <summary>
    /// Resolves a stored asset by its opaque token.
    /// </summary>
    Task<AssetFileRecord?> GetAsync(string token, CancellationToken cancellationToken);
}

namespace AppleWalletPass;

/// <summary>
/// Defines pass generation operations for the Wallet designer workflow.
/// </summary>
public interface IWalletPassGenerationService
{
    /// <summary>
    /// Generates a signed Wallet pass package for the supplied design model.
    /// </summary>
    Task<(byte[] FileBytes, string FileName)> GenerateAsync(PassDesignerModel design, CancellationToken cancellationToken);
}

namespace AppleWalletPass;

/// <summary>
/// Defines storage operations for Wallet signing settings used by the designer.
/// </summary>
public interface IWalletSigningSettingsStore
{
    /// <summary>
    /// Loads the current settings view model.
    /// </summary>
    Task<WalletSigningSettingsViewModel> GetAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Saves the current signing settings and optional certificate content.
    /// </summary>
    Task SaveAsync(
        WalletSigningSettingsInput input,
        Stream? certificateStream,
        CancellationToken cancellationToken);
}

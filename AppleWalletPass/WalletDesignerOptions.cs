#pragma warning disable CS1591
namespace SPC.Infrastructure.AppleWalletPass;

public sealed class WalletDesignerOptions
{
    public const string SectionName = "WalletDesigner";

    public string AssetStoragePath { get; set; } = "App_Data/assets";

    public string SettingsStoragePath { get; set; } = "App_Data/settings/wallet-settings.json";

    public string WwdrCertificatePath { get; set; } = "App_Data/certs/wwdr.pem";
}
#pragma warning restore CS1591

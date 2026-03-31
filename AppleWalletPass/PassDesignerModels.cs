#pragma warning disable CS1591
using System.ComponentModel.DataAnnotations;

namespace AppleWalletPass;

public enum DesignerPassType
{
    Boarding,
    Event,
    Coupon,
    StoreCard
}

public enum DesignerBarcodeStyle
{
    None,
    QrCode,
    Pdf417,
    Aztec,
    Code128
}

public enum BackFieldType
{
    PlainText,
    Link,
    MultilineText
}

public sealed class PassFieldInputModel
{
    public string Key { get; set; } = Guid.NewGuid().ToString("N")[..8];

    [Required]
    public string Label { get; set; } = string.Empty;

    [Required]
    public string Value { get; set; } = string.Empty;
}

public sealed class BackFieldInputModel
{
    public string Key { get; set; } = Guid.NewGuid().ToString("N")[..8];

    [Required]
    public string Label { get; set; } = string.Empty;

    [Required]
    public string Value { get; set; } = string.Empty;

    [Required]
    public BackFieldType Type { get; set; } = BackFieldType.PlainText;
}

public sealed class DesignerAssetReference
{
    public DesignerAssetSlot Slot { get; set; }

    public string? Token { get; set; }

    public string? FileName { get; set; }

    public string? PreviewUrl { get; set; }

    public string? ContentType { get; set; }
}

public sealed class PassDesignerModel
{
    [Required]
    public DesignerPassType PassType { get; set; } = DesignerPassType.StoreCard;

    [Required]
    [StringLength(255)]
    public string SerialNumber { get; set; } = $"PASS-{Guid.NewGuid():N}"[..17];

    [Required]
    [StringLength(128)]
    public string OrganizationName { get; set; } = "Acme Airlines";

    [Required]
    [StringLength(128)]
    public string Title { get; set; } = "Rewards Card";

    [StringLength(128)]
    public string Subtitle { get; set; } = "Gold Member";

    [Required]
    public string BackgroundColor { get; set; } = "#0A3D62";

    [Required]
    public DesignerBarcodeStyle BarcodeStyle { get; set; } = DesignerBarcodeStyle.QrCode;

    [StringLength(2048)]
    public string? BarcodeMessage { get; set; }

    [StringLength(255)]
    public string? BarcodeAltText { get; set; }

    public List<PassFieldInputModel> Fields { get; set; } =
    [
        new() { Label = "Customer", Value = "Eric Ngo" },
        new() { Label = "MemberSince", Value = "Apr 12" }
    ];

    public List<BackFieldInputModel> BackFields { get; set; } =
    [
        new() { Label = "Website", Value = "https://example.com/pass", Type = BackFieldType.Link },
        new() { Label = "Support", Value = "support@example.com", Type = BackFieldType.PlainText }
    ];

    public DesignerAssetReference IconAsset { get; set; } = new() { Slot = DesignerAssetSlot.Icon };

    public DesignerAssetReference LogoAsset { get; set; } = new() { Slot = DesignerAssetSlot.Logo };

    public DesignerAssetReference BackgroundAsset { get; set; } = new() { Slot = DesignerAssetSlot.Background };
}

public class WalletSigningSettingsInput
{
    [Required]
    [StringLength(128)]
    public string PassTypeIdentifier { get; set; } = string.Empty;

    [Required]
    [StringLength(64)]
    public string TeamIdentifier { get; set; } = string.Empty;

    [StringLength(128)]
    public string? DefaultOrganizationName { get; set; }

    [Required]
    [StringLength(256)]
    public string CertificatePassword { get; set; } = string.Empty;
}

public sealed class WalletSigningSettingsViewModel : WalletSigningSettingsInput
{
    public bool HasCertificate { get; set; }

    public DateTimeOffset? UpdatedAtUtc { get; set; }
}

public sealed class WalletSigningSettingsRecord
{
    public string? EncryptedCertificateBase64 { get; set; }

    public string? EncryptedCertificatePassword { get; set; }

    public string? PassTypeIdentifier { get; set; }

    public string? TeamIdentifier { get; set; }

    public string? DefaultOrganizationName { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}

public sealed class WalletSigningSettingsResolved
{
    public required byte[] CertificateBytes { get; init; }

    public required string CertificatePassword { get; init; }

    public required string PassTypeIdentifier { get; init; }

    public required string TeamIdentifier { get; init; }

    public string? DefaultOrganizationName { get; init; }

    public DateTimeOffset UpdatedAtUtc { get; init; }
}

public sealed class GeneratePassRequest
{
    public required PassDesignerModel Design { get; init; }
}
#pragma warning restore CS1591

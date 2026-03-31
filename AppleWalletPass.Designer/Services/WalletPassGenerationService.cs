using System.Security.Cryptography.X509Certificates;
using AppleWalletPass.Designer.Configuration;
using AppleWalletPass.Designer.Models;
using AppleWalletPass.Models;
using Microsoft.Extensions.Options;

namespace AppleWalletPass.Designer.Services;

public sealed class WalletPassGenerationService(
    WalletSigningSettingsStore settingsStore,
    DesignerAssetStore assetStore,
    IOptions<WalletDesignerOptions> options,
    IWebHostEnvironment environment)
{
    private readonly WalletSigningSettingsStore _settingsStore = settingsStore;
    private readonly DesignerAssetStore _assetStore = assetStore;
    private readonly WalletDesignerOptions _options = options.Value;
    private readonly IWebHostEnvironment _environment = environment;
    private readonly PassSigner _signer = new();
    private readonly PassPackager _packager = new();

    public async Task<(byte[] FileBytes, string FileName)> GenerateAsync(PassDesignerModel design, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(design);

        var settings = await _settingsStore.GetResolvedAsync(cancellationToken).ConfigureAwait(false);
        var wwdrPath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, _options.WwdrCertificatePath));
        if (!File.Exists(wwdrPath))
        {
            throw new InvalidOperationException($"The app-managed WWDR certificate was not found at '{wwdrPath}'. Configure WalletDesigner:WwdrCertificatePath before generating passes.");
        }

        var iconAsset = await RequireAssetAsync(design.IconAsset.Token, "icon", cancellationToken).ConfigureAwait(false);
        var logoAsset = await TryGetAssetAsync(design.LogoAsset.Token, cancellationToken).ConfigureAwait(false);
        var backgroundAsset = await TryGetAssetAsync(design.BackgroundAsset.Token, cancellationToken).ConfigureAwait(false);

        var pass = BuildPass(design, settings);
        var bundleFiles = new Dictionary<string, byte[]>(StringComparer.Ordinal)
        {
            ["pass.json"] = PassJson.SerializeToUtf8Bytes(pass),
            ["icon.png"] = await File.ReadAllBytesAsync(iconAsset.FilePath, cancellationToken).ConfigureAwait(false),
            ["icon@2x.png"] = await File.ReadAllBytesAsync(iconAsset.FilePath, cancellationToken).ConfigureAwait(false)
        };

        if (logoAsset is not null)
        {
            var logoBytes = await File.ReadAllBytesAsync(logoAsset.FilePath, cancellationToken).ConfigureAwait(false);
            bundleFiles["logo.png"] = logoBytes;
            bundleFiles["logo@2x.png"] = logoBytes;
        }

        if (backgroundAsset is not null)
        {
            bundleFiles["background.png"] = await File.ReadAllBytesAsync(backgroundAsset.FilePath, cancellationToken).ConfigureAwait(false);
        }

        using var signerCert = new X509Certificate2(settings.CertificateBytes, settings.CertificatePassword, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);
        using var wwdrCert = await LoadWwdrAsync(wwdrPath, cancellationToken).ConfigureAwait(false);
        bundleFiles["manifest.json"] = _signer.BuildManifest(bundleFiles);
        bundleFiles["signature"] = _signer.Sign(
            bundleFiles
                .Where(static pair => pair.Key is not "manifest.json" and not "signature")
                .ToDictionary(static pair => pair.Key, static pair => pair.Value, StringComparer.Ordinal),
            signerCert,
            wwdrCert);

        var fileBytes = _packager.Package(new PassBundle(bundleFiles));
        return (fileBytes, $"{SanitizeFileName(design.SerialNumber)}.pkpass");
    }

    private static string SanitizeFileName(string serialNumber)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(serialNumber.Select(ch => invalid.Contains(ch) ? '-' : ch).ToArray());
        return string.IsNullOrWhiteSpace(cleaned) ? $"pass-{Guid.NewGuid():N}" : cleaned;
    }

    private Pass BuildPass(PassDesignerModel design, WalletSigningSettingsResolved settings)
    {
        var builder = new PassBuilder()
            .WithOrganization(
                string.IsNullOrWhiteSpace(design.OrganizationName) ? settings.DefaultOrganizationName ?? "Apple Wallet Pass" : design.OrganizationName,
                settings.TeamIdentifier,
                settings.PassTypeIdentifier,
                $"{design.PassType} pass")
            .WithDescription($"{design.PassType} pass for {design.Title}")
            .WithSerial(design.SerialNumber)
            .WithLogoText(design.Title)
            .WithColors(design.BackgroundColor, "#FFFFFF", "#B4C8DC");

        builder = design.PassType switch
        {
            DesignerPassType.Boarding => builder.AsBoardingPass(PKTransitType.Air),
            DesignerPassType.Event => builder.AsEventTicket(),
            DesignerPassType.Coupon => builder.AsCoupon(),
            DesignerPassType.StoreCard => builder.AsStoreCard(),
            _ => builder.AsStoreCard()
        };

        builder.AddPrimaryField("title", "TITLE", design.Title);

        if (!string.IsNullOrWhiteSpace(design.Subtitle))
        {
            builder.AddSecondaryField("subtitle", "SUBTITLE", design.Subtitle);
        }

        foreach (var field in design.Fields.Where(static f => !string.IsNullOrWhiteSpace(f.Label) && !string.IsNullOrWhiteSpace(f.Value)).Take(8))
        {
            builder.AddAuxiliaryField(Slugify(field.Label), field.Label.ToUpperInvariant(), field.Value);
        }

        foreach (var field in design.BackFields.Where(static f => !string.IsNullOrWhiteSpace(f.Label) && !string.IsNullOrWhiteSpace(f.Value)).Take(12))
        {
            builder.AddBackField(Slugify(field.Label), field.Label, field.Value);
        }

        switch (design.BarcodeStyle)
        {
            case DesignerBarcodeStyle.None:
                break;
            case DesignerBarcodeStyle.Pdf417:
                builder.WithBarcode(BarcodeFormat.PDF417, GetBarcodeMessage(design), GetBarcodeAltText(design));
                break;
            case DesignerBarcodeStyle.Aztec:
                builder.WithBarcode(BarcodeFormat.Aztec, GetBarcodeMessage(design), GetBarcodeAltText(design));
                break;
            case DesignerBarcodeStyle.Code128:
                builder.WithBarcode(BarcodeFormat.Code128, GetBarcodeMessage(design), GetBarcodeAltText(design));
                break;
            default:
                builder.WithQRBarcode(GetBarcodeMessage(design), GetBarcodeAltText(design));
                break;
        }

        return builder.Build();
    }

    private static string Slugify(string value)
    {
        var chars = value
            .Where(static c => char.IsLetterOrDigit(c))
            .Select(static c => char.ToLowerInvariant(c))
            .ToArray();

        return chars.Length == 0 ? Guid.NewGuid().ToString("N")[..8] : new string(chars);
    }

    private static string GetBarcodeMessage(PassDesignerModel design)
        => string.IsNullOrWhiteSpace(design.BarcodeMessage) ? design.SerialNumber : design.BarcodeMessage.Trim();

    private static string GetBarcodeAltText(PassDesignerModel design)
        => string.IsNullOrWhiteSpace(design.BarcodeAltText) ? design.SerialNumber : design.BarcodeAltText.Trim();

    private async Task<AssetFileRecord?> TryGetAssetAsync(string? token, CancellationToken cancellationToken)
        => string.IsNullOrWhiteSpace(token) ? null : await _assetStore.GetAsync(token, cancellationToken).ConfigureAwait(false);

    private async Task<AssetFileRecord> RequireAssetAsync(string? token, string slotName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException($"Upload an {slotName} image before generating the pass.");
        }

        var asset = await _assetStore.GetAsync(token, cancellationToken).ConfigureAwait(false);
        return asset ?? throw new InvalidOperationException($"The uploaded {slotName} asset could not be found. Upload it again.");
    }

    private static async Task<X509Certificate2> LoadWwdrAsync(string path, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(path);
        if (extension.Equals(".pem", StringComparison.OrdinalIgnoreCase))
        {
            var pem = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
            return X509Certificate2.CreateFromPem(pem);
        }

        var bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        return new X509Certificate2(bytes);
    }
}

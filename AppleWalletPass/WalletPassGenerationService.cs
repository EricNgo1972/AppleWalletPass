#pragma warning disable CS1591
using AppleWalletPass.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AppleWalletPass;

public sealed class WalletPassGenerationService
{
    private readonly WalletSigningSettingsStore _settingsStore;
    private readonly DesignerAssetStore _assetStore;
    private readonly WalletDesignerOptions _options;
    private readonly IHostEnvironment _environment;

    public WalletPassGenerationService(
        WalletSigningSettingsStore settingsStore,
        DesignerAssetStore assetStore,
        IOptions<WalletDesignerOptions> options,
        IHostEnvironment environment)
    {
        _settingsStore = settingsStore;
        _assetStore = assetStore;
        _options = options.Value;
        _environment = environment;
    }

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

        var pass = BuildPass(design, settings, iconAsset, logoAsset, backgroundAsset);
        var signerPath = await WriteTemporaryCertificateAsync(settings.CertificateBytes, cancellationToken).ConfigureAwait(false);

        try
        {
            var generator = new PassGenerator(new PassGeneratorOptions
            {
                P12CertificatePath = signerPath,
                P12Passphrase = settings.CertificatePassword,
                WwdrCertificatePath = wwdrPath,
                OutputDirectory = null
            });

            var fileBytes = await generator.GenerateAsync(pass, cancellationToken).ConfigureAwait(false);
            return (fileBytes, $"{SanitizeFileName(design.SerialNumber)}.pkpass");
        }
        finally
        {
            TryDeleteFile(signerPath);
        }
    }

    private static string SanitizeFileName(string serialNumber)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(serialNumber.Select(ch => invalid.Contains(ch) ? '-' : ch).ToArray());
        return string.IsNullOrWhiteSpace(cleaned) ? $"pass-{Guid.NewGuid():N}" : cleaned;
    }

    private Pass BuildPass(
        PassDesignerModel design,
        WalletSigningSettingsResolved settings,
        AssetFileRecord iconAsset,
        AssetFileRecord? logoAsset,
        AssetFileRecord? backgroundAsset)
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

        builder.WithIcon(iconAsset.FilePath, iconAsset.FilePath);

        if (logoAsset is not null)
        {
            builder.WithLogo(logoAsset.FilePath, logoAsset.FilePath);
        }

        if (backgroundAsset is not null)
        {
            builder.WithBackground(backgroundAsset.FilePath);
        }

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
        => string.IsNullOrWhiteSpace(token) ? null : await _assetStore.GetRecordAsync(token, cancellationToken).ConfigureAwait(false);

    private async Task<AssetFileRecord> RequireAssetAsync(string? token, string slotName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException($"Upload an {slotName} image before generating the pass.");
        }

        var asset = await _assetStore.GetRecordAsync(token, cancellationToken).ConfigureAwait(false);
        return asset ?? throw new InvalidOperationException($"The uploaded {slotName} asset could not be found. Upload it again.");
    }

    private async Task<string> WriteTemporaryCertificateAsync(byte[] certificateBytes, CancellationToken cancellationToken)
    {
        var tempDirectory = Path.Combine(_environment.ContentRootPath, "App_Data", "temp");
        Directory.CreateDirectory(tempDirectory);

        var path = Path.Combine(tempDirectory, $"{Guid.NewGuid():N}.p12");
        await File.WriteAllBytesAsync(path, certificateBytes, cancellationToken).ConfigureAwait(false);
        return path;
    }

    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
        }
    }
}
#pragma warning restore CS1591

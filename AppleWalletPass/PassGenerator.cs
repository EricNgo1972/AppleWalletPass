using System.Security.Cryptography.X509Certificates;
using AppleWalletPass.Models;

namespace AppleWalletPass;

/// <summary>
/// Coordinates pass validation, signing, and packaging.
/// </summary>
public class PassGenerator
{
    private readonly PassGeneratorOptions _options;
    private readonly PassSigner _signer;
    private readonly PassPackager _packager;

    /// <summary>
    /// Initializes a new instance of the <see cref="PassGenerator"/> class.
    /// </summary>
    /// <param name="options">The generator options.</param>
    public PassGenerator(PassGeneratorOptions options)
        : this(options, new PassSigner(), new PassPackager())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PassGenerator"/> class.
    /// </summary>
    /// <param name="options">The generator options.</param>
    /// <param name="signer">The signer instance.</param>
    /// <param name="packager">The packager instance.</param>
    internal PassGenerator(PassGeneratorOptions options, PassSigner signer, PassPackager packager)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _signer = signer ?? throw new ArgumentNullException(nameof(signer));
        _packager = packager ?? throw new ArgumentNullException(nameof(packager));
    }

    /// <summary>
    /// Generates a signed .pkpass archive.
    /// </summary>
    /// <param name="pass">The pass to generate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The packaged .pkpass bytes.</returns>
    public async Task<byte[]> GenerateAsync(Pass pass, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pass);
        PassValidator.ValidateForGeneration(pass);

        var signerCert = await LoadSignerCertificateAsync(cancellationToken).ConfigureAwait(false);
        var wwdrCert = await LoadWwdrCertificateAsync(cancellationToken).ConfigureAwait(false);

        var bundleFiles = new Dictionary<string, byte[]>(StringComparer.Ordinal)
        {
            ["pass.json"] = PassJson.SerializeToUtf8Bytes(pass),
            ["icon.png"] = await ReadImageAsync(pass.Images.IconPath!, "icon.png", cancellationToken).ConfigureAwait(false)
        };

        bundleFiles["icon@2x.png"] = pass.Images.Icon2xPath is { Length: > 0 }
            ? await ReadImageAsync(pass.Images.Icon2xPath, "icon@2x.png", cancellationToken).ConfigureAwait(false)
            : bundleFiles["icon.png"];

        if (!string.IsNullOrWhiteSpace(pass.Images.LogoPath))
        {
            bundleFiles["logo.png"] = await ReadImageAsync(pass.Images.LogoPath, "logo.png", cancellationToken).ConfigureAwait(false);
            bundleFiles["logo@2x.png"] = !string.IsNullOrWhiteSpace(pass.Images.Logo2xPath)
                ? await ReadImageAsync(pass.Images.Logo2xPath, "logo@2x.png", cancellationToken).ConfigureAwait(false)
                : bundleFiles["logo.png"];
        }

        if (!string.IsNullOrWhiteSpace(pass.Images.BackgroundPath))
        {
            bundleFiles["background.png"] = await ReadImageAsync(pass.Images.BackgroundPath, "background.png", cancellationToken).ConfigureAwait(false);
        }

        var manifestBytes = _signer.BuildManifest(bundleFiles);
        var signatureBytes = _signer.Sign(bundleFiles, signerCert, wwdrCert);

        var packagedFiles = new Dictionary<string, byte[]>(bundleFiles, StringComparer.Ordinal)
        {
            ["manifest.json"] = manifestBytes,
            ["signature"] = signatureBytes
        };

        var pkpass = _packager.Package(new PassBundle(packagedFiles));

        if (!string.IsNullOrWhiteSpace(_options.OutputDirectory))
        {
            Directory.CreateDirectory(_options.OutputDirectory);
            var outputPath = Path.Combine(_options.OutputDirectory, $"{pass.SerialNumber}.pkpass");
            await File.WriteAllBytesAsync(outputPath, pkpass, cancellationToken).ConfigureAwait(false);
        }

        return pkpass;
    }

    private async Task<X509Certificate2> LoadSignerCertificateAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_options.P12CertificatePath))
        {
            throw new PassGenerationException(
                PassGenerationErrorCode.CertificateNotFound,
                $"Signer certificate file was not found at '{_options.P12CertificatePath}'.");
        }

        var rawBytes = await File.ReadAllBytesAsync(_options.P12CertificatePath, cancellationToken).ConfigureAwait(false);
        var certificate = new X509Certificate2(rawBytes, _options.P12Passphrase, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);

        if (!certificate.HasPrivateKey)
        {
            throw new PassGenerationException(
                PassGenerationErrorCode.SigningFailed,
                "The signer certificate does not contain a private key.");
        }

        return certificate;
    }

    private async Task<X509Certificate2> LoadWwdrCertificateAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_options.WwdrCertificatePath))
        {
            throw new PassGenerationException(
                PassGenerationErrorCode.CertificateNotFound,
                $"WWDR certificate file was not found at '{_options.WwdrCertificatePath}'.");
        }

        var extension = Path.GetExtension(_options.WwdrCertificatePath);
        if (extension.Equals(".pem", StringComparison.OrdinalIgnoreCase))
        {
            var pem = await File.ReadAllTextAsync(_options.WwdrCertificatePath, cancellationToken).ConfigureAwait(false);
            return X509Certificate2.CreateFromPem(pem);
        }

        var rawBytes = await File.ReadAllBytesAsync(_options.WwdrCertificatePath, cancellationToken).ConfigureAwait(false);
        return new X509Certificate2(rawBytes);
    }

    private static async Task<byte[]> ReadImageAsync(string path, string assetName, CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            throw new PassGenerationException(
                PassGenerationErrorCode.ImageMissing,
                $"Required asset '{assetName}' was not found at '{path}'.");
        }

        return await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
    }
}

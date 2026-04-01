using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace SPC.Infrastructure.AppleWalletPass;

/// <summary>
/// Creates the Wallet manifest and detached PKCS#7 signature.
/// </summary>
internal class PassSigner
{
    /// <summary>
    /// Builds manifest.json bytes using SHA-1 hashes for every bundle file.
    /// </summary>
    /// <param name="bundleFiles">The bundle files to hash.</param>
    /// <returns>The manifest.json UTF-8 bytes.</returns>
    public virtual byte[] BuildManifest(IReadOnlyDictionary<string, byte[]> bundleFiles)
    {
        ArgumentNullException.ThrowIfNull(bundleFiles);

        var manifest = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var (fileName, content) in bundleFiles)
        {
            manifest[fileName] = Convert.ToHexString(SHA1.HashData(content)).ToLowerInvariant();
        }

        return JsonSerializer.SerializeToUtf8Bytes(manifest, new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        });
    }

    /// <summary>
    /// Produces a detached PKCS#7 signature for the manifest content derived from the bundle files.
    /// </summary>
    /// <param name="bundleFiles">The files that will be included in the bundle before manifest and signature.</param>
    /// <param name="signerCert">The signer certificate containing a private key.</param>
    /// <param name="wwdrCert">The Apple WWDR intermediate certificate.</param>
    /// <returns>The CMS signature bytes.</returns>
    public virtual byte[] Sign(
        Dictionary<string, byte[]> bundleFiles,
        X509Certificate2 signerCert,
        X509Certificate2 wwdrCert)
    {
        ArgumentNullException.ThrowIfNull(bundleFiles);
        ArgumentNullException.ThrowIfNull(signerCert);
        ArgumentNullException.ThrowIfNull(wwdrCert);

        EnsureCertificateIsCurrentlyValid(signerCert, nameof(signerCert));
        EnsureCertificateIsCurrentlyValid(wwdrCert, nameof(wwdrCert));

        try
        {
            var manifestBytes = BuildManifest(bundleFiles);
            var contentInfo = new ContentInfo(manifestBytes);
            var signedCms = new SignedCms(contentInfo, detached: true);
            var cmsSigner = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, signerCert)
            {
                IncludeOption = X509IncludeOption.EndCertOnly,
                DigestAlgorithm = new Oid("1.3.14.3.2.26")
            };

            cmsSigner.Certificates.Add(wwdrCert);
            signedCms.ComputeSignature(cmsSigner);
            return signedCms.Encode();
        }
        catch (PassGenerationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new PassGenerationException(
                PassGenerationErrorCode.SigningFailed,
                "Failed to create the detached PKCS#7 signature.",
                ex);
        }
    }

    private static void EnsureCertificateIsCurrentlyValid(X509Certificate2 certificate, string name)
    {
        var utcNow = DateTimeOffset.UtcNow;
        if (utcNow < certificate.NotBefore.ToUniversalTime() || utcNow > certificate.NotAfter.ToUniversalTime())
        {
            throw new PassGenerationException(
                PassGenerationErrorCode.CertificateExpired,
                $"The certificate '{name}' is not currently valid. Valid from {certificate.NotBefore:u} to {certificate.NotAfter:u}.");
        }
    }
}

using System.IO.Compression;
using SPC.Infrastructure.AppleWalletPass.Models;

namespace SPC.Infrastructure.AppleWalletPass;

/// <summary>
/// Packages Wallet pass bundle files into a .pkpass archive.
/// </summary>
internal class PassPackager
{
    private static readonly string[] RequiredEntries =
    [
        "pass.json",
        "manifest.json",
        "signature",
        "icon.png",
        "icon@2x.png"
    ];

    /// <summary>
    /// Packages the supplied pass bundle into a ZIP archive.
    /// </summary>
    /// <param name="bundle">The bundle to package.</param>
    /// <returns>The ZIP archive bytes.</returns>
    public virtual byte[] Package(PassBundle bundle)
    {
        ArgumentNullException.ThrowIfNull(bundle);

        foreach (var fileName in RequiredEntries)
        {
            if (!bundle.Files.ContainsKey(fileName))
            {
                throw new PassGenerationException(
                    PassGenerationErrorCode.InvalidPassData,
                    $"Bundle is missing required entry '{fileName}'.");
            }
        }

        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var (fileName, content) in bundle.Files.OrderBy(static entry => entry.Key, StringComparer.Ordinal))
            {
                var entry = archive.CreateEntry(fileName, CompressionLevel.Optimal);
                using var entryStream = entry.Open();
                entryStream.Write(content, 0, content.Length);
            }
        }

        return stream.ToArray();
    }
}

using System.IO.Compression;
using System.Text;
using AppleWalletPass.Models;

namespace AppleWalletPass.Tests;

public class PassPackagerTests
{
    [Fact]
    public void Package_CreatesZipWithExpectedEntries()
    {
        var bundle = new PassBundle(new Dictionary<string, byte[]>
        {
            ["pass.json"] = Encoding.UTF8.GetBytes("{}"),
            ["manifest.json"] = Encoding.UTF8.GetBytes("{}"),
            ["signature"] = [1, 2, 3],
            ["icon.png"] = [4, 5, 6],
            ["icon@2x.png"] = [7, 8, 9],
            ["logo.png"] = [10],
            ["logo@2x.png"] = [11]
        });

        var package = new PassPackager().Package(bundle);

        using var stream = new MemoryStream(package);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

        Assert.NotNull(archive.GetEntry("pass.json"));
        Assert.NotNull(archive.GetEntry("manifest.json"));
        Assert.NotNull(archive.GetEntry("signature"));
        Assert.NotNull(archive.GetEntry("icon.png"));
        Assert.NotNull(archive.GetEntry("icon@2x.png"));
        Assert.NotNull(archive.GetEntry("logo.png"));
        Assert.NotNull(archive.GetEntry("logo@2x.png"));
    }
}

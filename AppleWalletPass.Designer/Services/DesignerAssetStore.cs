using System.Text.Json;
using AppleWalletPass.Designer.Configuration;
using AppleWalletPass.Designer.Models;
using Microsoft.Extensions.Options;

namespace AppleWalletPass.Designer.Services;

public sealed class DesignerAssetStore(IWebHostEnvironment environment, IOptions<WalletDesignerOptions> options)
{
    private readonly IWebHostEnvironment _environment = environment;
    private readonly WalletDesignerOptions _options = options.Value;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public async Task<AssetUploadResponse> SaveAsync(
        DesignerAssetSlot slot,
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        var token = Guid.NewGuid().ToString("N");
        var rootPath = GetAssetRoot();
        Directory.CreateDirectory(rootPath);

        var sanitizedExtension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(sanitizedExtension))
        {
            sanitizedExtension = ".bin";
        }

        var filePath = Path.Combine(rootPath, $"{token}{sanitizedExtension}");
        var metadataPath = GetMetadataPath(token);

        await using (var target = File.Create(filePath))
        {
            await content.CopyToAsync(target, cancellationToken).ConfigureAwait(false);
        }

        var record = new AssetFileRecord
        {
            Token = token,
            FilePath = filePath,
            FileName = fileName,
            ContentType = contentType,
            Slot = slot
        };

        await File.WriteAllTextAsync(metadataPath, JsonSerializer.Serialize(record, _jsonOptions), cancellationToken).ConfigureAwait(false);

        return new AssetUploadResponse
        {
            Token = token,
            FileName = fileName,
            ContentType = contentType,
            PreviewUrl = $"/api/assets/{token}"
        };
    }

    public async Task<AssetFileRecord?> GetAsync(string token, CancellationToken cancellationToken)
    {
        var metadataPath = GetMetadataPath(token);
        if (!File.Exists(metadataPath))
        {
            return null;
        }

        var json = await File.ReadAllTextAsync(metadataPath, cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<AssetFileRecord>(json, _jsonOptions);
    }

    private string GetAssetRoot()
        => Path.GetFullPath(Path.Combine(_environment.ContentRootPath, _options.AssetStoragePath));

    private string GetMetadataPath(string token)
        => Path.Combine(GetAssetRoot(), $"{token}.json");
}

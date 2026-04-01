using SPC.Infrastructure.AppleWalletPass;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SPC.UI.Blazor.AppleWalletPass.Controllers;

[ApiController]
[Route("api/assets")]
public sealed class DesignerAssetsController(DesignerAssetStore assetStore) : ControllerBase
{
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png",
        ".jpg",
        ".jpeg",
        ".gif",
        ".webp"
    };

    [HttpPost("upload/{slot}")]
    [RequestSizeLimit(10_000_000)]
    public async Task<ActionResult<AssetUploadResponse>> UploadAsync(DesignerAssetSlot slot, IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "Select an image file to upload." });
        }

        var hasImageContentType = !string.IsNullOrWhiteSpace(file.ContentType) &&
            file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
        var hasImageExtension = AllowedImageExtensions.Contains(Path.GetExtension(file.FileName));

        if (!hasImageContentType && !hasImageExtension)
        {
            return BadRequest(new { message = "Only image uploads are supported for pass assets." });
        }

        await using var stream = file.OpenReadStream();
        var normalizedContentType = hasImageContentType
            ? file.ContentType
            : GetContentTypeFromExtension(file.FileName);

        var result = await assetStore.SaveAsync(slot, stream, file.FileName, normalizedContentType, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("{token}")]
    public async Task<IActionResult> GetAsync(string token, CancellationToken cancellationToken)
    {
        var asset = await assetStore.GetContentAsync(token, cancellationToken).ConfigureAwait(false);
        if (asset is null)
        {
            return NotFound();
        }

        return File(asset.Value.Content, asset.Value.ContentType, enableRangeProcessing: false);
    }

    private static string GetContentTypeFromExtension(string fileName)
        => Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
}

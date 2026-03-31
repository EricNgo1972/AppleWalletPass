using AppleWalletPass.Designer.Models;
using AppleWalletPass.Designer.Services;
using Microsoft.AspNetCore.Mvc;
using WalletPassLib = AppleWalletPass;

namespace AppleWalletPass.Designer.Controllers;

[ApiController]
[Route("api/passes")]
public sealed class DesignerPassesController(WalletPassGenerationService generationService) : ControllerBase
{
    [HttpPost("download")]
    public async Task<IActionResult> DownloadAsync([FromBody] GeneratePassRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await generationService.GenerateAsync(request.Design, cancellationToken).ConfigureAwait(false);
            return File(result.FileBytes, "application/vnd.apple.pkpass", result.FileName);
        }
        catch (Exception ex) when (ex is InvalidOperationException or WalletPassLib.PassGenerationException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

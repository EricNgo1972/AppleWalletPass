using SPC.Infrastructure.AppleWalletPass;
using Microsoft.AspNetCore.Mvc;
using SPC.UI.Blazor.AppleWalletPass.Models;
using WalletPassLib = SPC.Infrastructure.AppleWalletPass;

namespace SPC.UI.Blazor.AppleWalletPass.Controllers;

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

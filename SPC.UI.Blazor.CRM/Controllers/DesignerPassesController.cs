using AppleWalletPass;
using Microsoft.AspNetCore.Mvc;
using WalletPassLib = AppleWalletPass;

namespace SPC.UI.Blazor.CRM.Controllers;

[ApiController]
[Route("api/passes")]
public sealed class DesignerPassesController(IWalletPassGenerationService generationService) : ControllerBase
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

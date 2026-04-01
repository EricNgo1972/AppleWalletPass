using SPC.Infrastructure.AppleWalletPass;

namespace SPC.UI.Blazor.AppleWalletPass.Models;

public sealed class GeneratePassRequest
{
    public required PassDesignerModel Design { get; init; }
}

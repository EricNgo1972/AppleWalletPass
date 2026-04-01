using AppleWalletPass;

namespace SPC.UI.Blazor.CRM.Models;

public sealed class GeneratePassRequest
{
    public required PassDesignerModel Design { get; init; }
}

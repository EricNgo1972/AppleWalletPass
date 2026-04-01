using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using SPC.UI.Blazor.AppleWalletPass.Controllers;
using SPC.UI.Blazor.AppleWalletPass.Services;

namespace SPC.UI.Blazor.AppleWalletPass;

/// <summary>
/// Service registration helpers for CRM UI and controller features.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers CRM UI services owned by the Razor class library.
    /// </summary>
    public static IServiceCollection AddCrmUiServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<BarcodePreviewRenderer>();
        return services;
    }

    /// <summary>
    /// Adds CRM controller parts from the Razor class library.
    /// </summary>
    public static IMvcBuilder AddCrmUiControllers(this IMvcBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddApplicationPart(typeof(DesignerAssetsController).Assembly);
        return builder;
    }
}

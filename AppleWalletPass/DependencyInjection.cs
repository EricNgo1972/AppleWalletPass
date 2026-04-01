using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppleWalletPass;

/// <summary>
/// Service registration helpers for the non-UI Wallet designer infrastructure.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Wallet designer infrastructure services owned by the core library.
    /// </summary>
    public static IServiceCollection AddAppleWalletPassDesignerCore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<WalletDesignerOptions>(configuration.GetSection(WalletDesignerOptions.SectionName));
        services.AddSingleton<DesignerAssetStore>();
        services.AddSingleton<WalletSigningSettingsStore>();
        services.AddScoped<WalletPassGenerationService>();

        return services;
    }
}

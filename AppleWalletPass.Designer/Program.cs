using AppleWalletPass.Designer.Components;
using AppleWalletPass.Designer.Configuration;
using AppleWalletPass.Designer.Controllers;
using AppleWalletPass.Designer.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<WalletDesignerOptions>(builder.Configuration.GetSection(WalletDesignerOptions.SectionName));
builder.Services.AddDataProtection();
builder.Services.AddMudServices();
builder.Services.AddControllers().AddApplicationPart(typeof(DesignerAssetsController).Assembly);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped(sp =>
{
    var navigationManager = sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navigationManager.BaseUri) };
});
builder.Services.AddSingleton<ThemeState>();
builder.Services.AddSingleton<BarcodePreviewRenderer>();
builder.Services.AddSingleton<DesignerAssetStore>();
builder.Services.AddSingleton<WalletSigningSettingsStore>();
builder.Services.AddScoped<WalletPassGenerationService>();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

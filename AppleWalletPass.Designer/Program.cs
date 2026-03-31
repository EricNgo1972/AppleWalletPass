using AppleWalletPass;
using SPC.UI.Blazor.CRM;
using SPC.UI.Blazor.CRM.Services;
using AppleWalletPass.Designer.Components;
using AppleWalletPass.Designer.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection();
builder.Services.AddMudServices();
builder.Services.AddControllers().AddCrmUiControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped(sp =>
{
    var navigationManager = sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navigationManager.BaseUri) };
});
builder.Services.AddSingleton<ThemeState>();
builder.Services.AddCrmUiServices();
builder.Services.AddAppleWalletPassDesignerCore(builder.Configuration);
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
    .AddAdditionalAssemblies(typeof(SPC.UI.Blazor.CRM.Components.Pages.Designer).Assembly)
    .AddInteractiveServerRenderMode();

app.Run();

using MudBlazor;

namespace AppleWalletPass.Designer.Services;

public sealed class ThemeState
{
    public event Action? Changed;

    public bool IsDarkMode { get; private set; } = true;

    public MudTheme CurrentTheme => IsDarkMode ? DarkTheme : LightTheme;

    public void Toggle()
    {
        IsDarkMode = !IsDarkMode;
        Changed?.Invoke();
    }

    public static MudTheme DarkTheme { get; } = new()
    {
        PaletteDark = new PaletteDark
        {
            Primary = "#5DA9FF",
            Secondary = "#F97066",
            Background = "#131516",
            Surface = "#1B1D1F",
            AppbarBackground = "#111315",
            DrawerBackground = "#17191B",
            DrawerText = "#E9ECEF",
            TextPrimary = "#F5F7FA",
            TextSecondary = "#B7C0CC",
            LinesDefault = "rgba(255,255,255,0.08)"
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "20px"
        }
    };

    public static MudTheme LightTheme { get; } = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#145DA0",
            Secondary = "#C84B31",
            Background = "#F3F6FA",
            Surface = "#FFFFFF",
            AppbarBackground = "rgba(255,255,255,0.92)",
            DrawerBackground = "#EEF3F8",
            DrawerText = "#18324A",
            TextPrimary = "#17212B",
            TextSecondary = "#526172",
            LinesDefault = "rgba(18,29,43,0.08)"
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "20px"
        }
    };
}

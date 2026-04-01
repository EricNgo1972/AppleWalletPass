# AppleWalletPass User Guide

## Overview

This solution has three distinct layers:

- [AppleWalletPass](/mnt/c/SPC/spc-walletpass/AppleWalletPass): non-UI Wallet pass library
- [AppleWalletPass.Designer](/mnt/c/SPC/spc-walletpass/AppleWalletPass.Designer): Blazor Server host shell
- [SPC.UI.Blazor.CRM](/mnt/c/SPC/spc-walletpass/SPC.UI.Blazor.CRM): Razor Class Library for the designer UI

The core library builds Apple Wallet payloads, signs them with Apple-issued certificates, and packages them into `.pkpass` files. The Blazor app uses that library to provide a browser-based pass designer.

## Architecture

### `AppleWalletPass`

Use this project when you need programmatic pass generation.

Public reusable API:

- `PassBuilder`
- `PassGenerator`
- `PassGenerationException`
- `PassGenerationErrorCode`
- domain types in [PassModels.cs](/mnt/c/SPC/spc-walletpass/AppleWalletPass/Models/PassModels.cs)

Public designer-facing contracts kept for cross-project use:

- [IDesignerAssetStore.cs](/mnt/c/SPC/spc-walletpass/AppleWalletPass/DesignerAssetContracts.cs)
- [IWalletSigningSettingsStore.cs](/mnt/c/SPC/spc-walletpass/AppleWalletPass/IWalletSigningSettingsStore.cs)
- [IWalletPassGenerationService.cs](/mnt/c/SPC/spc-walletpass/AppleWalletPass/IWalletPassGenerationService.cs)

Internal implementation details:

- signing implementation
- ZIP packaging implementation
- validation implementation
- asset storage implementation
- signing-settings persistence implementation
- app-specific generation service implementation

This keeps the core library usable without exposing every internal workflow class.

### `AppleWalletPass.Designer`

Use this project as the app host. It owns:

- `Program.cs`
- root `App.razor`
- route shell
- main layout and navigation
- theme state
- host configuration

### `SPC.UI.Blazor.AppleWalletPass`

Use this project for the designer experience. It owns:

- route pages
- reusable designer components
- preview components
- CRM controllers
- preview barcode rendering
- CRM static web assets

## Core Library Usage

### Build a pass with `PassBuilder`

Supported style selectors:

- `AsBoardingPass(PKTransitType transitType)`
- `AsEventTicket()`
- `AsCoupon()`
- `AsStoreCard()`
- `AsGeneric()`

Common metadata methods:

- `WithOrganization(string organizationName, string teamId, string passTypeId, string? description = null)`
- `WithDescription(string description)`
- `WithSerial(string serialNumber)`
- `WithLogoText(string logoText)`
- `WithColors(string background, string foreground, string label)`
- `WithRelevantDate(DateTimeOffset relevantDate)`

Barcode methods:

- `WithQRBarcode(string message, string? altText = null)`
- `WithBarcode(BarcodeFormat format, string message, string? altText = null)`

Asset methods:

- `WithIcon(string iconPath, string? icon2xPath = null)`
- `WithLogo(string logoPath, string? logo2xPath = null)`
- `WithBackground(string backgroundPath)`

Field methods:

- `AddHeaderField(...)`
- `AddPrimaryField(...)`
- `AddSecondaryField(...)`
- `AddAuxiliaryField(...)`
- `AddBackField(...)`

Context methods:

- `AddLocation(...)`
- `AddBeacon(...)`

Finalize with:

- `Build()`

### Example: Boarding pass

```csharp
using AppleWalletPass;
using AppleWalletPass.Models;

var pass = new PassBuilder()
    .AsBoardingPass(PKTransitType.Air)
    .WithOrganization("Acme Airlines", "ABC123XYZ", "pass.com.acme.boarding", "Boarding pass")
    .WithSerial("PKP-2024-0394-7821")
    .WithColors("#0A3D62", "#FFFFFF", "#B4C8DC")
    .WithIcon("./assets/icon.png", "./assets/icon@2x.png")
    .WithLogo("./assets/logo.png", "./assets/logo@2x.png")
    .AddPrimaryField("origin", "FROM", "JFK")
    .AddPrimaryField("destination", "TO", "LAX")
    .AddSecondaryField("flight", "FLIGHT", "AC102")
    .AddSecondaryField("seat", "SEAT", "14A")
    .AddAuxiliaryField("gate", "GATE", "22")
    .AddBackField("terms", "TERMS", "Boarding begins 30 minutes before departure.")
    .WithQRBarcode("PKP-2024-0394-7821")
    .Build();
```

### Generate with `PassGenerator`

```csharp
var generator = new PassGenerator(new PassGeneratorOptions
{
    P12CertificatePath = "./certs/pass.p12",
    P12Passphrase = "my-passphrase",
    WwdrCertificatePath = "./certs/wwdr.pem",
    OutputDirectory = "./output"
});

byte[] pkpass = await generator.GenerateAsync(pass);
```

`PassGenerator` is the public top-level generation API. Lower-level classes such as signing, packaging, and validation are internal implementation details.

## Domain Model Notes

Key types in [PassModels.cs](/mnt/c/SPC/spc-walletpass/AppleWalletPass/Models/PassModels.cs):

- `Pass`
- `PassStyle`
- `BoardingPassStyle`
- `PassField`
- `Barcode`
- `PassColors`
- `Location`
- `BeaconInfo`
- `PassImagePaths`
- `PassGeneratorOptions`

Supported enums:

- `PassKind`
- `PKTransitType`
- `BarcodeFormat`
- `PassTextAlignment`
- `PassDateStyle`
- `PassNumberStyle`

## Validation And Errors

The generation flow validates:

- required Wallet identifiers
- serial number presence and length
- organization name and description
- normalized color format
- required icon assets

The library throws [PassGenerationException.cs](/mnt/c/SPC/spc-walletpass/AppleWalletPass/PassGenerationException.cs) with:

- `CertificateNotFound`
- `CertificateExpired`
- `SigningFailed`
- `InvalidPassData`
- `ImageMissing`

Typical handling:

```csharp
try
{
    var pkpass = await generator.GenerateAsync(pass);
}
catch (PassGenerationException ex) when (ex.ErrorCode == PassGenerationErrorCode.InvalidPassData)
{
    // return validation details to the caller
}
```

## Designer App

The Blazor designer supports:

- boarding, event, coupon, and store card presets
- front-face and back-detail editing
- Wallet-style preview
- separate barcode payload and display caption
- uploaded icon, logo, and background assets
- direct `.pkpass` download

### Routes

- `/`
- `/passes/designer`
- `/passes/settings`

### Theme Ownership

The shell theme is maintained by [MainLayout.razor](/mnt/c/SPC/spc-walletpass/AppleWalletPass.Designer/Components/Layout/MainLayout.razor), [Routes.razor](/mnt/c/SPC/spc-walletpass/AppleWalletPass.Designer/Components/Routes.razor), and [ThemeState.cs](/mnt/c/SPC/spc-walletpass/AppleWalletPass.Designer/Services/ThemeState.cs) in the host project.

The pass preview is designed to remain Wallet-like and not fully inherit the app theme.

### Settings

The signing settings workflow stores:

- `.p12` certificate content
- certificate password
- `passTypeIdentifier`
- `teamIdentifier`
- optional default organization name

The Apple Wallet WWDR intermediate is app-managed and not user-uploaded.

## Dependency Injection

The projects now register their own implementations:

- [AppleWalletPass/DependencyInjection.cs](/mnt/c/SPC/spc-walletpass/AppleWalletPass/DependencyInjection.cs)
- [SPC.UI.Blazor.CRM/DependencyInjection.cs](/mnt/c/SPC/spc-walletpass/SPC.UI.Blazor.CRM/DependencyInjection.cs)

The host in [Program.cs](/mnt/c/SPC/spc-walletpass/AppleWalletPass.Designer/Program.cs) composes those registrations.

## Testing

Current tests cover:

- pass builder JSON structure
- validator required field handling
- ZIP package entry creation
- integration outline for sign + package flow

Run:

```bash
'/mnt/c/Program Files/dotnet/dotnet.exe' build AppleWalletPass.sln -p:UseAppHost=false
'/mnt/c/Program Files/dotnet/dotnet.exe' test AppleWalletPass.Tests/AppleWalletPass.Tests.csproj --no-build
```

## Notes For Maintainers

- Keep UI-specific request models in the UI project unless they are true shared contracts.
- Keep `AppleWalletPass` focused on reusable pass generation and only expose contracts the UI actually needs.
- Prefer `PassBuilder` and `PassGenerator` as the supported integration path for external consumers.

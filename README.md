# AppleWalletPass

`AppleWalletPass` is a `.NET 8` solution for generating signed Apple Wallet `.pkpass` files and hosting a Blazor Server pass designer.

## Solution Layout

```text
AppleWalletPass.sln
AppleWalletPass/            Core Wallet pass library
AppleWalletPass.Designer/   Blazor Server host shell
SPC.UI.Blazor.CRM/          Razor Class Library for designer UI
AppleWalletPass.Tests/      xUnit tests
README.md
userguide.md
```

## Projects

### `AppleWalletPass`

Non-UI core library for:

- building `pass.json` with `PassBuilder`
- validating pass content before generation
- loading certificates and images
- creating `manifest.json`
- signing with detached PKCS#7 / CMS
- packaging `.pkpass` archives

The public API is intentionally narrow. External callers should normally use:

- `PassBuilder`
- `PassGenerator`
- types in `AppleWalletPass.Models`
- `PassGenerationException`

The library also exposes only the minimum service contracts currently used by the designer app:

- `IDesignerAssetStore`
- `IWalletSigningSettingsStore`
- `IWalletPassGenerationService`

Low-level implementation types such as signing, packaging, validation, and storage implementations are internal.

### `AppleWalletPass.Designer`

Blazor Server host shell that owns:

- app startup and dependency injection composition
- root app component and routing shell
- layout and theme state
- host configuration and app settings

### `SPC.UI.Blazor.AppleWalletPass`

Razor Class Library that owns:

- pass designer pages and reusable Razor components
- CRM-facing controllers
- preview rendering helpers
- CRM UI static assets

## Core Library Quick Start

### Requirements

- .NET SDK 8.0+
- Apple Wallet pass certificate as `.p12` or `.pfx`
- Apple Wallet WWDR intermediate certificate as `.pem` or `.cer`
- required pass icon assets:
  - `icon.png`
  - `icon@2x.png`

### Build a pass

```csharp
using AppleWalletPass;
using AppleWalletPass.Models;

var pass = new PassBuilder()
    .AsBoardingPass(PKTransitType.Air)
    .WithOrganization("Acme Airlines", teamId: "ABC123XYZ", passTypeId: "pass.com.acme.boarding")
    .WithDescription("Acme Airlines boarding pass")
    .WithSerial("PKP-2024-0394-7821")
    .WithColors(background: "#0A3D62", foreground: "#FFFFFF", label: "#B4C8DC")
    .AddPrimaryField("origin", "FROM", "JFK")
    .AddPrimaryField("destination", "TO", "LAX")
    .AddSecondaryField("date", "DATE", "Apr 12")
    .AddSecondaryField("seat", "SEAT", "14A")
    .WithQRBarcode("PKP-2024-0394-7821")
    .WithIcon("./assets/icon.png", "./assets/icon@2x.png")
    .WithLogo("./assets/logo.png", "./assets/logo@2x.png")
    .Build();
```

### Generate `.pkpass`

```csharp
var generator = new PassGenerator(new PassGeneratorOptions
{
    P12CertificatePath = "./certs/pass.p12",
    P12Passphrase = "my-passphrase",
    WwdrCertificatePath = "./certs/wwdr.pem",
    OutputDirectory = "./output"
});

byte[] pkpass = await generator.GenerateAsync(pass);
await File.WriteAllBytesAsync("boarding.pkpass", pkpass);
```

## Supported Pass Types

- `AsBoardingPass(PKTransitType transitType)`
- `AsEventTicket()`
- `AsCoupon()`
- `AsStoreCard()`
- `AsGeneric()`

## Blazor Designer App

The solution also includes a Blazor Server designer with:

- live front/back pass editing
- Wallet-style preview
- light/dark app theme switching
- image upload for Wallet assets
- server-side `.pkpass` generation and download

Main routes:

- `/`
- `/passes/designer`
- `/passes/settings`

## Validation And Errors

The generation path validates:

- `passTypeIdentifier`
- `teamIdentifier`
- `serialNumber`
- `organizationName`
- `description`
- serial number length `<= 255`
- valid normalized color values
- presence of required icon assets

Generation failures use `PassGenerationException` with:

- `CertificateNotFound`
- `CertificateExpired`
- `SigningFailed`
- `InvalidPassData`
- `ImageMissing`

## Build And Test

```bash
'/mnt/c/Program Files/dotnet/dotnet.exe' build AppleWalletPass.sln -p:UseAppHost=false
'/mnt/c/Program Files/dotnet/dotnet.exe' test AppleWalletPass.Tests/AppleWalletPass.Tests.csproj --no-build
```

## Additional Documentation

See [userguide.md](/mnt/c/SPC/spc-walletpass/userguide.md) for the API guide, architecture notes, and Blazor designer details.

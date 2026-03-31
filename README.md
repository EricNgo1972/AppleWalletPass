# AppleWalletPass

`AppleWalletPass` is a `.NET 8` class library for generating signed Apple Wallet `.pkpass` files using only platform libraries plus `System.Security.Cryptography.Pkcs`.

## What It Includes

- Fluent `PassBuilder` API for all Wallet pass styles
- Strongly typed pass models and Apple-specific enums
- SHA-1 `manifest.json` generation
- Detached PKCS#7 signing
- ZIP packaging for `.pkpass` output
- Validation with descriptive error codes
- Async file IO for certificates and image assets
- xUnit test coverage and integration test outline

## Solution Layout

```text
AppleWalletPass.sln
AppleWalletPass/
  Models/PassModels.cs
  PassBuilder.cs
  PassGenerationException.cs
  PassGenerator.cs
  PassPackager.cs
  PassSigner.cs
  PassValidator.cs
AppleWalletPass.Tests/
README.md
userguide.md
```

## Requirements

- .NET SDK 8.0+
- Apple Wallet pass certificate exported as `.p12` or `.pfx`
- Apple WWDR intermediate certificate as `.pem` or `.cer`
- Required pass icons:
  - `icon.png`
  - `icon@2x.png`

## Quick Start

### 1. Prepare assets

```text
certs/
  pass.p12
  wwdr.pem
assets/
  icon.png
  icon@2x.png
  logo.png
  logo@2x.png
```

### 2. Build a pass

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

### 3. Generate `.pkpass`

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

## ASP.NET Core Example

```csharp
app.MapPost("/passes/generate", async (PassRequest req, PassGenerator gen) =>
{
    var pkpass = await gen.GenerateAsync(req.ToPass());
    return Results.File(
        pkpass,
        contentType: "application/vnd.apple.pkpass",
        fileDownloadName: $"{req.Serial}.pkpass");
});
```

## Validation Rules

- `passTypeIdentifier` is required
- `teamIdentifier` is required
- `serialNumber` is required
- `organizationName` is required
- `description` is required
- serial number length must be `<= 255`
- colors must be valid CSS `rgb(r, g, b)` strings after normalization
- `icon.png` must be present before generation

## Error Handling

The library throws `PassGenerationException` with these error codes:

- `CertificateNotFound`
- `CertificateExpired`
- `SigningFailed`
- `InvalidPassData`
- `ImageMissing`

## Build And Test

```bash
'/mnt/c/Program Files/dotnet/dotnet.exe' build AppleWalletPass.sln
'/mnt/c/Program Files/dotnet/dotnet.exe' test AppleWalletPass.Tests/AppleWalletPass.Tests.csproj
```

## Additional Documentation

See [userguide.md](/mnt/c/SPC/spc-walletpass/userguide.md) for the full API guide, usage patterns, validation behavior, and implementation notes.

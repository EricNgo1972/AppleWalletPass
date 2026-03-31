# AppleWalletPass User Guide

## Overview

`AppleWalletPass` builds Apple Wallet payloads, signs them with your pass certificate, and packages them into `.pkpass` ZIP archives.

The generation flow is:

1. Build a `Pass` object with `PassBuilder`
2. Validate required fields and image configuration
3. Serialize `pass.json`
4. Hash bundle files into `manifest.json`
5. Create a detached PKCS#7 `signature`
6. Package everything into a `.pkpass`

## Core Types

### `PassBuilder`

Main fluent API for constructing pass data.

Supported style selectors:

- `AsBoardingPass(PKTransitType transitType)`
- `AsEventTicket()`
- `AsCoupon()`
- `AsStoreCard()`
- `AsGeneric()`

Metadata methods:

- `WithOrganization(string organizationName, string teamId, string passTypeId, string? description = null)`
- `WithDescription(string description)`
- `WithSerial(string serialNumber)`
- `WithLogoText(string logoText)`
- `WithColors(string background, string foreground, string label)`
- `WithRelevantDate(DateTimeOffset relevantDate)`

Barcode methods:

- `WithQRBarcode(string message, string? altText = null)`
- `WithBarcode(BarcodeFormat format, string message, string? altText = null)`

Image methods:

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

- `AddLocation(double latitude, double longitude, double? altitude = null, string? relevantText = null)`
- `AddBeacon(Guid proximityUuid, ushort? major = null, ushort? minor = null, string? relevantText = null)`

Finalize:

- `Build()`

## Example: Boarding Pass

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

## Example: Event Ticket

```csharp
var pass = new PassBuilder()
    .AsEventTicket()
    .WithOrganization("Acme Live", "ABC123XYZ", "pass.com.acme.event", "Concert ticket")
    .WithSerial("EVT-2026-0001")
    .WithIcon("./assets/icon.png", "./assets/icon@2x.png")
    .AddPrimaryField("event", "EVENT", "Acme Live 2026")
    .AddSecondaryField("venue", "VENUE", "Grand Hall")
    .AddSecondaryField("seat", "SEAT", "B14")
    .WithBarcode(BarcodeFormat.Aztec, "EVT-2026-0001")
    .Build();
```

## Pass Models

### `PassField`

Represents one field inside the pass layout.

Properties:

- `key`
- `label`
- `value`
- `textAlignment`
- `dateStyle`
- `numberStyle`

### `Barcode`

Represents barcode payload data.

Formats:

- `QR`
- `PDF417`
- `Aztec`
- `Code128`

### `PassColors`

The builder normalizes `#RGB` and `#RRGGBB` input into CSS `rgb(r, g, b)` strings.

### `Location`

Represents a Wallet-relevant location trigger.

### `BeaconInfo`

Represents iBeacon metadata used for proximity-based display.

## Signing

`PassSigner` is responsible for:

- hashing each bundle file with SHA-1
- building `manifest.json`
- creating a detached CMS / PKCS#7 signature

Signature method:

```csharp
byte[] Sign(
    Dictionary<string, byte[]> bundleFiles,
    X509Certificate2 signerCert,
    X509Certificate2 wwdrCert);
```

Notes:

- `bundleFiles` should contain the unsigned payload files such as `pass.json`, icons, logos, and background
- `manifest.json` is derived from those files
- the CMS signature is detached and signs the manifest content
- the signer certificate must include a private key

## Packaging

`PassPackager` writes the final bundle with `ZipArchive`.

Expected entries:

- `pass.json`
- `manifest.json`
- `signature`
- `icon.png`
- `icon@2x.png`
- `logo.png` if provided
- `logo@2x.png` if provided
- `background.png` if provided

Packaging method:

```csharp
byte[] Package(PassBundle bundle);
```

## Top-Level Generation

`PassGenerator` loads certificates, reads assets, signs the manifest, and packages the pass.

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

If `OutputDirectory` is set, the generator also writes `{serialNumber}.pkpass` to disk.

## Validation

`PassValidator` enforces:

- `passTypeIdentifier` is present
- `teamIdentifier` is present
- `serialNumber` is present
- `organizationName` is present
- `description` is present
- serial number length does not exceed 255 characters
- colors are valid CSS `rgb(...)` strings
- `icon.png` is configured before generation

Validation methods:

```csharp
PassValidator.Validate(pass);
PassValidator.ValidateForGeneration(pass);
```

## Exceptions

All generation failures use `PassGenerationException`.

Available `ErrorCode` values:

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
    // return validation error to caller
}
```

## ASP.NET Core Endpoint

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

Recommended response headers:

- `Content-Type: application/vnd.apple.pkpass`
- `Content-Disposition: attachment; filename="<serial>.pkpass"`

## Testing

Included tests cover:

- builder JSON structure
- validator required field handling
- ZIP archive entry creation
- full integration outline for real certificates

Run tests with:

```bash
'/mnt/c/Program Files/dotnet/dotnet.exe' test AppleWalletPass.Tests/AppleWalletPass.Tests.csproj
```

## Operational Notes

- Keep Apple certificates out of source control
- Provide real `icon.png` and `icon@2x.png` assets for every generated pass
- Use a stable serial number strategy on your side
- Reuse `PassGenerator` through dependency injection in web apps
- Enable the skipped integration test only in an environment with valid Apple-issued credentials

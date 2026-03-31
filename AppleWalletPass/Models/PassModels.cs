using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AppleWalletPass.Models;

/// <summary>
/// Represents a Wallet pass type.
/// </summary>
public enum PassKind
{
    /// <summary>
    /// A boarding pass.
    /// </summary>
    BoardingPass,

    /// <summary>
    /// An event ticket.
    /// </summary>
    EventTicket,

    /// <summary>
    /// A coupon.
    /// </summary>
    Coupon,

    /// <summary>
    /// A store card.
    /// </summary>
    StoreCard,

    /// <summary>
    /// A generic pass.
    /// </summary>
    Generic
}

/// <summary>
/// Represents Apple Wallet transit types.
/// </summary>
[JsonConverter(typeof(PKTransitTypeJsonConverter))]
public enum PKTransitType
{
    /// <summary>
    /// Air transit.
    /// </summary>
    Air,

    /// <summary>
    /// Boat transit.
    /// </summary>
    Boat,

    /// <summary>
    /// Bus transit.
    /// </summary>
    Bus,

    /// <summary>
    /// Generic transit.
    /// </summary>
    Generic,

    /// <summary>
    /// Train transit.
    /// </summary>
    Train
}

/// <summary>
/// Represents Apple Wallet barcode formats.
/// </summary>
[JsonConverter(typeof(BarcodeFormatJsonConverter))]
public enum BarcodeFormat
{
    /// <summary>
    /// QR barcode.
    /// </summary>
    QR,

    /// <summary>
    /// PDF417 barcode.
    /// </summary>
    PDF417,

    /// <summary>
    /// Aztec barcode.
    /// </summary>
    Aztec,

    /// <summary>
    /// Code 128 barcode.
    /// </summary>
    Code128
}

/// <summary>
/// Represents Apple Wallet text alignment values.
/// </summary>
[JsonConverter(typeof(PassTextAlignmentJsonConverter))]
public enum PassTextAlignment
{
    /// <summary>
    /// Natural alignment.
    /// </summary>
    Natural,

    /// <summary>
    /// Left alignment.
    /// </summary>
    Left,

    /// <summary>
    /// Center alignment.
    /// </summary>
    Center,

    /// <summary>
    /// Right alignment.
    /// </summary>
    Right
}

/// <summary>
/// Represents Apple Wallet date styles.
/// </summary>
[JsonConverter(typeof(PassDateStyleJsonConverter))]
public enum PassDateStyle
{
    /// <summary>
    /// No date style.
    /// </summary>
    None,

    /// <summary>
    /// Short date style.
    /// </summary>
    Short,

    /// <summary>
    /// Medium date style.
    /// </summary>
    Medium,

    /// <summary>
    /// Long date style.
    /// </summary>
    Long,

    /// <summary>
    /// Full date style.
    /// </summary>
    Full
}

/// <summary>
/// Represents Apple Wallet number styles.
/// </summary>
[JsonConverter(typeof(PassNumberStyleJsonConverter))]
public enum PassNumberStyle
{
    /// <summary>
    /// Decimal style.
    /// </summary>
    Decimal,

    /// <summary>
    /// Percent style.
    /// </summary>
    Percent,

    /// <summary>
    /// Scientific style.
    /// </summary>
    Scientific,

    /// <summary>
    /// Spell-out style.
    /// </summary>
    SpellOut
}

/// <summary>
/// Represents a pass field entry.
/// </summary>
/// <param name="Key">The field key.</param>
/// <param name="Label">The field label.</param>
/// <param name="Value">The field value.</param>
/// <param name="TextAlignment">The text alignment.</param>
/// <param name="DateStyle">The date style.</param>
/// <param name="NumberStyle">The number style.</param>
public sealed record PassField(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("label")] string? Label,
    [property: JsonPropertyName("value")] object? Value,
    [property: JsonPropertyName("textAlignment")] PassTextAlignment? TextAlignment = null,
    [property: JsonPropertyName("dateStyle")] PassDateStyle? DateStyle = null,
    [property: JsonPropertyName("numberStyle")] PassNumberStyle? NumberStyle = null);

/// <summary>
/// Represents barcode information.
/// </summary>
/// <param name="Format">The barcode format.</param>
/// <param name="Message">The barcode message.</param>
/// <param name="AltText">Optional alternate text.</param>
/// <param name="MessageEncoding">The message encoding.</param>
public sealed record Barcode(
    [property: JsonPropertyName("format")] BarcodeFormat Format,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("altText")] string? AltText = null,
    [property: JsonPropertyName("messageEncoding")] string MessageEncoding = "iso-8859-1");

/// <summary>
/// Represents pass colors in CSS rgb syntax.
/// </summary>
/// <param name="BackgroundColor">The background color.</param>
/// <param name="ForegroundColor">The foreground color.</param>
/// <param name="LabelColor">The label color.</param>
public sealed record PassColors(
    [property: JsonPropertyName("backgroundColor")] string BackgroundColor,
    [property: JsonPropertyName("foregroundColor")] string ForegroundColor,
    [property: JsonPropertyName("labelColor")] string LabelColor);

/// <summary>
/// Represents a relevant geographic location.
/// </summary>
/// <param name="Latitude">The latitude.</param>
/// <param name="Longitude">The longitude.</param>
/// <param name="Altitude">The altitude.</param>
/// <param name="RelevantText">The relevant text.</param>
public sealed record Location(
    [property: JsonPropertyName("latitude")] double Latitude,
    [property: JsonPropertyName("longitude")] double Longitude,
    [property: JsonPropertyName("altitude")] double? Altitude = null,
    [property: JsonPropertyName("relevantText")] string? RelevantText = null);

/// <summary>
/// Represents iBeacon metadata.
/// </summary>
/// <param name="ProximityUuid">The beacon UUID.</param>
/// <param name="Major">The major identifier.</param>
/// <param name="Minor">The minor identifier.</param>
/// <param name="RelevantText">The relevant text.</param>
public sealed record BeaconInfo(
    [property: JsonPropertyName("proximityUUID")] Guid ProximityUuid,
    [property: JsonPropertyName("major")] ushort? Major = null,
    [property: JsonPropertyName("minor")] ushort? Minor = null,
    [property: JsonPropertyName("relevantText")] string? RelevantText = null);

/// <summary>
/// Represents image asset paths used during packaging.
/// </summary>
public sealed record PassImagePaths
{
    /// <summary>
    /// Gets or initializes the icon asset path.
    /// </summary>
    public string? IconPath { get; init; }

    /// <summary>
    /// Gets or initializes the retina icon asset path.
    /// </summary>
    public string? Icon2xPath { get; init; }

    /// <summary>
    /// Gets or initializes the logo asset path.
    /// </summary>
    public string? LogoPath { get; init; }

    /// <summary>
    /// Gets or initializes the retina logo asset path.
    /// </summary>
    public string? Logo2xPath { get; init; }

    /// <summary>
    /// Gets or initializes the background asset path.
    /// </summary>
    public string? BackgroundPath { get; init; }
}

/// <summary>
/// Represents a set of pass fields.
/// </summary>
public sealed class PassFieldSet
{
    /// <summary>
    /// Gets the header fields.
    /// </summary>
    [JsonPropertyName("headerFields")]
    public List<PassField> HeaderFields { get; } = [];

    /// <summary>
    /// Gets the primary fields.
    /// </summary>
    [JsonPropertyName("primaryFields")]
    public List<PassField> PrimaryFields { get; } = [];

    /// <summary>
    /// Gets the secondary fields.
    /// </summary>
    [JsonPropertyName("secondaryFields")]
    public List<PassField> SecondaryFields { get; } = [];

    /// <summary>
    /// Gets the auxiliary fields.
    /// </summary>
    [JsonPropertyName("auxiliaryFields")]
    public List<PassField> AuxiliaryFields { get; } = [];

    /// <summary>
    /// Gets the back fields.
    /// </summary>
    [JsonPropertyName("backFields")]
    public List<PassField> BackFields { get; } = [];
}

/// <summary>
/// Represents a generic pass type section.
/// </summary>
public class PassStyle
{
    /// <summary>
    /// Gets the header fields.
    /// </summary>
    [JsonPropertyName("headerFields")]
    public List<PassField> HeaderFields { get; } = [];

    /// <summary>
    /// Gets the primary fields.
    /// </summary>
    [JsonPropertyName("primaryFields")]
    public List<PassField> PrimaryFields { get; } = [];

    /// <summary>
    /// Gets the secondary fields.
    /// </summary>
    [JsonPropertyName("secondaryFields")]
    public List<PassField> SecondaryFields { get; } = [];

    /// <summary>
    /// Gets the auxiliary fields.
    /// </summary>
    [JsonPropertyName("auxiliaryFields")]
    public List<PassField> AuxiliaryFields { get; } = [];

    /// <summary>
    /// Gets the back fields.
    /// </summary>
    [JsonPropertyName("backFields")]
    public List<PassField> BackFields { get; } = [];
}

/// <summary>
/// Represents a boarding pass section.
/// </summary>
public sealed class BoardingPassStyle : PassStyle
{
    /// <summary>
    /// Gets or sets the transit type.
    /// </summary>
    [JsonPropertyName("transitType")]
    public PKTransitType TransitType { get; set; }
}

/// <summary>
/// Represents a pass bundle ready for ZIP packaging.
/// </summary>
public sealed record PassBundle(IReadOnlyDictionary<string, byte[]> Files);

/// <summary>
/// Represents generator configuration.
/// </summary>
public sealed record PassGeneratorOptions
{
    /// <summary>
    /// Gets or initializes the .p12/.pfx certificate path.
    /// </summary>
    public required string P12CertificatePath { get; init; }

    /// <summary>
    /// Gets or initializes the certificate passphrase.
    /// </summary>
    public required string P12Passphrase { get; init; }

    /// <summary>
    /// Gets or initializes the Apple WWDR certificate path.
    /// </summary>
    public required string WwdrCertificatePath { get; init; }

    /// <summary>
    /// Gets or initializes the optional output directory.
    /// </summary>
    public string? OutputDirectory { get; init; }
}

/// <summary>
/// Represents a fully built Apple Wallet pass definition.
/// </summary>
public sealed class Pass
{
    /// <summary>
    /// Gets the Wallet format version.
    /// </summary>
    [JsonPropertyName("formatVersion")]
    public int FormatVersion { get; } = 1;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the organization name.
    /// </summary>
    [JsonPropertyName("organizationName")]
    public string? OrganizationName { get; set; }

    /// <summary>
    /// Gets or sets the pass type identifier.
    /// </summary>
    [JsonPropertyName("passTypeIdentifier")]
    public string? PassTypeIdentifier { get; set; }

    /// <summary>
    /// Gets or sets the team identifier.
    /// </summary>
    [JsonPropertyName("teamIdentifier")]
    public string? TeamIdentifier { get; set; }

    /// <summary>
    /// Gets or sets the serial number.
    /// </summary>
    [JsonPropertyName("serialNumber")]
    public string? SerialNumber { get; set; }

    /// <summary>
    /// Gets or sets the optional logo text.
    /// </summary>
    [JsonPropertyName("logoText")]
    public string? LogoText { get; set; }

    /// <summary>
    /// Gets or sets the colors.
    /// </summary>
    [JsonPropertyName("backgroundColor")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BackgroundColor => Colors?.BackgroundColor;

    /// <summary>
    /// Gets or sets the foreground color.
    /// </summary>
    [JsonPropertyName("foregroundColor")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ForegroundColor => Colors?.ForegroundColor;

    /// <summary>
    /// Gets or sets the label color.
    /// </summary>
    [JsonPropertyName("labelColor")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LabelColor => Colors?.LabelColor;

    /// <summary>
    /// Gets or sets the color aggregate.
    /// </summary>
    [JsonIgnore]
    public PassColors? Colors { get; set; }

    /// <summary>
    /// Gets or sets the barcode.
    /// </summary>
    [JsonPropertyName("barcode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Barcode? Barcode { get; set; }

    /// <summary>
    /// Gets or sets the barcode collection.
    /// </summary>
    [JsonPropertyName("barcodes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Barcode>? Barcodes { get; set; }

    /// <summary>
    /// Gets the relevant locations.
    /// </summary>
    [JsonPropertyName("locations")]
    public List<Location> Locations { get; } = [];

    /// <summary>
    /// Gets the beacon metadata.
    /// </summary>
    [JsonPropertyName("beacons")]
    public List<BeaconInfo> Beacons { get; } = [];

    /// <summary>
    /// Gets or sets the relevant date.
    /// </summary>
    [JsonPropertyName("relevantDate")]
    public DateTimeOffset? RelevantDate { get; set; }

    /// <summary>
    /// Gets or sets the authentication token.
    /// </summary>
    [JsonPropertyName("authenticationToken")]
    public string? AuthenticationToken { get; set; }

    /// <summary>
    /// Gets or sets the web service URL.
    /// </summary>
    [JsonPropertyName("webServiceURL")]
    public string? WebServiceUrl { get; set; }

    /// <summary>
    /// Gets or sets the associated store identifiers.
    /// </summary>
    [JsonPropertyName("associatedStoreIdentifiers")]
    public List<int>? AssociatedStoreIdentifiers { get; set; }

    /// <summary>
    /// Gets or sets the boarding pass section.
    /// </summary>
    [JsonPropertyName("boardingPass")]
    public BoardingPassStyle? BoardingPass { get; set; }

    /// <summary>
    /// Gets or sets the event ticket section.
    /// </summary>
    [JsonPropertyName("eventTicket")]
    public PassStyle? EventTicket { get; set; }

    /// <summary>
    /// Gets or sets the coupon section.
    /// </summary>
    [JsonPropertyName("coupon")]
    public PassStyle? Coupon { get; set; }

    /// <summary>
    /// Gets or sets the store card section.
    /// </summary>
    [JsonPropertyName("storeCard")]
    public PassStyle? StoreCard { get; set; }

    /// <summary>
    /// Gets or sets the generic section.
    /// </summary>
    [JsonPropertyName("generic")]
    public PassStyle? Generic { get; set; }

    /// <summary>
    /// Gets or sets the pass kind.
    /// </summary>
    [JsonIgnore]
    public PassKind? Kind { get; set; }

    /// <summary>
    /// Gets or sets the image asset paths.
    /// </summary>
    [JsonIgnore]
    public PassImagePaths Images { get; set; } = new();
}

/// <summary>
/// Provides shared JSON serialization settings for Wallet pass payloads.
/// </summary>
public static class PassJson
{
    /// <summary>
    /// Gets the default serializer options.
    /// </summary>
    public static JsonSerializerOptions SerializerOptions { get; } = CreateOptions();

    /// <summary>
    /// Serializes a pass to JSON.
    /// </summary>
    /// <param name="pass">The pass to serialize.</param>
    /// <returns>The JSON string.</returns>
    public static string Serialize(Pass pass)
    {
        ArgumentNullException.ThrowIfNull(pass);
        return JsonSerializer.Serialize(pass, SerializerOptions);
    }

    /// <summary>
    /// Serializes a pass to UTF-8 bytes.
    /// </summary>
    /// <param name="pass">The pass to serialize.</param>
    /// <returns>The JSON payload bytes.</returns>
    public static byte[] SerializeToUtf8Bytes(Pass pass)
    {
        ArgumentNullException.ThrowIfNull(pass);
        return JsonSerializer.SerializeToUtf8Bytes(pass, SerializerOptions);
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };

        return options;
    }
}

internal sealed class PKTransitTypeJsonConverter : JsonConverter<PKTransitType>
{
    public override PKTransitType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.GetString() switch
        {
            "PKTransitTypeAir" => PKTransitType.Air,
            "PKTransitTypeBoat" => PKTransitType.Boat,
            "PKTransitTypeBus" => PKTransitType.Bus,
            "PKTransitTypeGeneric" => PKTransitType.Generic,
            "PKTransitTypeTrain" => PKTransitType.Train,
            _ => throw new JsonException("Unsupported transit type.")
        };

    public override void Write(Utf8JsonWriter writer, PKTransitType value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value switch
        {
            PKTransitType.Air => "PKTransitTypeAir",
            PKTransitType.Boat => "PKTransitTypeBoat",
            PKTransitType.Bus => "PKTransitTypeBus",
            PKTransitType.Generic => "PKTransitTypeGeneric",
            PKTransitType.Train => "PKTransitTypeTrain",
            _ => throw new JsonException("Unsupported transit type.")
        });
}

internal sealed class BarcodeFormatJsonConverter : JsonConverter<BarcodeFormat>
{
    public override BarcodeFormat Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.GetString() switch
        {
            "PKBarcodeFormatQR" => BarcodeFormat.QR,
            "PKBarcodeFormatPDF417" => BarcodeFormat.PDF417,
            "PKBarcodeFormatAztec" => BarcodeFormat.Aztec,
            "PKBarcodeFormatCode128" => BarcodeFormat.Code128,
            _ => throw new JsonException("Unsupported barcode format.")
        };

    public override void Write(Utf8JsonWriter writer, BarcodeFormat value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value switch
        {
            BarcodeFormat.QR => "PKBarcodeFormatQR",
            BarcodeFormat.PDF417 => "PKBarcodeFormatPDF417",
            BarcodeFormat.Aztec => "PKBarcodeFormatAztec",
            BarcodeFormat.Code128 => "PKBarcodeFormatCode128",
            _ => throw new JsonException("Unsupported barcode format.")
        });
}

internal sealed class PassTextAlignmentJsonConverter : JsonConverter<PassTextAlignment>
{
    public override PassTextAlignment Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.GetString() switch
        {
            "PKTextAlignmentLeft" => PassTextAlignment.Left,
            "PKTextAlignmentCenter" => PassTextAlignment.Center,
            "PKTextAlignmentRight" => PassTextAlignment.Right,
            "PKTextAlignmentNatural" => PassTextAlignment.Natural,
            _ => throw new JsonException("Unsupported text alignment.")
        };

    public override void Write(Utf8JsonWriter writer, PassTextAlignment value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value switch
        {
            PassTextAlignment.Left => "PKTextAlignmentLeft",
            PassTextAlignment.Center => "PKTextAlignmentCenter",
            PassTextAlignment.Right => "PKTextAlignmentRight",
            PassTextAlignment.Natural => "PKTextAlignmentNatural",
            _ => throw new JsonException("Unsupported text alignment.")
        });
}

internal sealed class PassDateStyleJsonConverter : JsonConverter<PassDateStyle>
{
    public override PassDateStyle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.GetString() switch
        {
            "PKDateStyleNone" => PassDateStyle.None,
            "PKDateStyleShort" => PassDateStyle.Short,
            "PKDateStyleMedium" => PassDateStyle.Medium,
            "PKDateStyleLong" => PassDateStyle.Long,
            "PKDateStyleFull" => PassDateStyle.Full,
            _ => throw new JsonException("Unsupported date style.")
        };

    public override void Write(Utf8JsonWriter writer, PassDateStyle value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value switch
        {
            PassDateStyle.None => "PKDateStyleNone",
            PassDateStyle.Short => "PKDateStyleShort",
            PassDateStyle.Medium => "PKDateStyleMedium",
            PassDateStyle.Long => "PKDateStyleLong",
            PassDateStyle.Full => "PKDateStyleFull",
            _ => throw new JsonException("Unsupported date style.")
        });
}

internal sealed class PassNumberStyleJsonConverter : JsonConverter<PassNumberStyle>
{
    public override PassNumberStyle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.GetString() switch
        {
            "PKNumberStyleDecimal" => PassNumberStyle.Decimal,
            "PKNumberStylePercent" => PassNumberStyle.Percent,
            "PKNumberStyleScientific" => PassNumberStyle.Scientific,
            "PKNumberStyleSpellOut" => PassNumberStyle.SpellOut,
            _ => throw new JsonException("Unsupported number style.")
        };

    public override void Write(Utf8JsonWriter writer, PassNumberStyle value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value switch
        {
            PassNumberStyle.Decimal => "PKNumberStyleDecimal",
            PassNumberStyle.Percent => "PKNumberStylePercent",
            PassNumberStyle.Scientific => "PKNumberStyleScientific",
            PassNumberStyle.SpellOut => "PKNumberStyleSpellOut",
            _ => throw new JsonException("Unsupported number style.")
        });
}

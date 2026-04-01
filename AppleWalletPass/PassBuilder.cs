using System.Globalization;
using System.Text.RegularExpressions;
using SPC.Infrastructure.AppleWalletPass.Models;

namespace SPC.Infrastructure.AppleWalletPass;

/// <summary>
/// Provides a fluent API for building Wallet pass payloads.
/// </summary>
public partial class PassBuilder
{
    private readonly Pass _pass = new();
    private PassStyle? _activeStyle;

    /// <summary>
    /// Configures the pass as a boarding pass.
    /// </summary>
    /// <param name="transitType">The transit type.</param>
    /// <returns>The current builder.</returns>
    public PassBuilder AsBoardingPass(PKTransitType transitType)
    {
        ClearStyles();
        _pass.Kind = PassKind.BoardingPass;
        _pass.BoardingPass = new BoardingPassStyle { TransitType = transitType };
        _activeStyle = _pass.BoardingPass;
        return this;
    }

    /// <summary>
    /// Configures the pass as an event ticket.
    /// </summary>
    /// <returns>The current builder.</returns>
    public PassBuilder AsEventTicket()
    {
        ClearStyles();
        _pass.Kind = PassKind.EventTicket;
        _pass.EventTicket = new PassStyle();
        _activeStyle = _pass.EventTicket;
        return this;
    }

    /// <summary>
    /// Configures the pass as a coupon.
    /// </summary>
    /// <returns>The current builder.</returns>
    public PassBuilder AsCoupon()
    {
        ClearStyles();
        _pass.Kind = PassKind.Coupon;
        _pass.Coupon = new PassStyle();
        _activeStyle = _pass.Coupon;
        return this;
    }

    /// <summary>
    /// Configures the pass as a store card.
    /// </summary>
    /// <returns>The current builder.</returns>
    public PassBuilder AsStoreCard()
    {
        ClearStyles();
        _pass.Kind = PassKind.StoreCard;
        _pass.StoreCard = new PassStyle();
        _activeStyle = _pass.StoreCard;
        return this;
    }

    /// <summary>
    /// Configures the pass as a generic pass.
    /// </summary>
    /// <returns>The current builder.</returns>
    public PassBuilder AsGeneric()
    {
        ClearStyles();
        _pass.Kind = PassKind.Generic;
        _pass.Generic = new PassStyle();
        _activeStyle = _pass.Generic;
        return this;
    }

    /// <summary>
    /// Sets organization metadata.
    /// </summary>
    /// <param name="organizationName">The organization name.</param>
    /// <param name="teamId">The Apple team identifier.</param>
    /// <param name="passTypeId">The Apple pass type identifier.</param>
    /// <param name="description">An optional pass description.</param>
    /// <returns>The current builder.</returns>
    public PassBuilder WithOrganization(string organizationName, string teamId, string passTypeId, string? description = null)
    {
        _pass.OrganizationName = organizationName;
        _pass.TeamIdentifier = teamId;
        _pass.PassTypeIdentifier = passTypeId;
        _pass.Description = string.IsNullOrWhiteSpace(description) ? $"{organizationName} pass" : description;
        return this;
    }

    /// <summary>
    /// Sets the pass description.
    /// </summary>
    /// <param name="description">The pass description.</param>
    /// <returns>The current builder.</returns>
    public PassBuilder WithDescription(string description)
    {
        _pass.Description = description;
        return this;
    }

    /// <summary>
    /// Sets the pass serial number.
    /// </summary>
    /// <param name="serialNumber">The serial number.</param>
    /// <returns>The current builder.</returns>
    public PassBuilder WithSerial(string serialNumber)
    {
        _pass.SerialNumber = serialNumber;
        return this;
    }

    /// <summary>
    /// Sets the pass logo text.
    /// </summary>
    /// <param name="logoText">The logo text.</param>
    /// <returns>The current builder.</returns>
    public PassBuilder WithLogoText(string logoText)
    {
        _pass.LogoText = logoText;
        return this;
    }

    /// <summary>
    /// Sets pass colors from hex or CSS rgb values.
    /// </summary>
    /// <param name="background">The background color.</param>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="label">The label color.</param>
    /// <returns>The current builder.</returns>
    public PassBuilder WithColors(string background, string foreground, string label)
    {
        _pass.Colors = new PassColors(
            NormalizeColor(background),
            NormalizeColor(foreground),
            NormalizeColor(label));
        return this;
    }

    /// <summary>
    /// Sets a QR barcode.
    /// </summary>
    /// <param name="message">The barcode message.</param>
    /// <param name="altText">Optional alternate text.</param>
    /// <returns>The current builder.</returns>
    public PassBuilder WithQRBarcode(string message, string? altText = null) =>
        WithBarcode(BarcodeFormat.QR, message, altText);

    /// <summary>
    /// Sets a barcode.
    /// </summary>
    /// <param name="format">The barcode format.</param>
    /// <param name="message">The barcode message.</param>
    /// <param name="altText">Optional alternate text.</param>
    /// <returns>The current builder.</returns>
    public PassBuilder WithBarcode(BarcodeFormat format, string message, string? altText = null)
    {
        var barcode = new Barcode(format, message, altText);
        _pass.Barcode = barcode;
        _pass.Barcodes = [barcode];
        return this;
    }

    /// <summary>
    /// Sets the base and optional retina icon image.
    /// </summary>
    /// <param name="iconPath">The icon path.</param>
    /// <param name="icon2xPath">The optional retina icon path.</param>
    /// <returns>The current builder.</returns>
    public PassBuilder WithIcon(string iconPath, string? icon2xPath = null)
    {
        _pass.Images = _pass.Images with
        {
            IconPath = iconPath,
            Icon2xPath = icon2xPath
        };

        return this;
    }

    /// <summary>
    /// Sets the base and optional retina logo image.
    /// </summary>
    /// <param name="logoPath">The logo path.</param>
    /// <param name="logo2xPath">The optional retina logo path.</param>
    /// <returns>The current builder.</returns>
    public PassBuilder WithLogo(string logoPath, string? logo2xPath = null)
    {
        _pass.Images = _pass.Images with
        {
            LogoPath = logoPath,
            Logo2xPath = logo2xPath
        };

        return this;
    }

    /// <summary>
    /// Sets the background image.
    /// </summary>
    /// <param name="backgroundPath">The background image path.</param>
    /// <returns>The current builder.</returns>
    public PassBuilder WithBackground(string backgroundPath)
    {
        _pass.Images = _pass.Images with
        {
            BackgroundPath = backgroundPath
        };

        return this;
    }

    /// <summary>
    /// Adds a header field.
    /// </summary>
    /// <param name="key">The field key.</param>
    /// <param name="label">The field label.</param>
    /// <param name="value">The field value.</param>
    /// <param name="textAlignment">Optional text alignment.</param>
    /// <param name="dateStyle">Optional date style.</param>
    /// <param name="numberStyle">Optional number style.</param>
    /// <returns>The current builder.</returns>
    public PassBuilder AddHeaderField(
        string key,
        string label,
        object value,
        PassTextAlignment? textAlignment = null,
        PassDateStyle? dateStyle = null,
        PassNumberStyle? numberStyle = null)
    {
        ActiveStyle().HeaderFields.Add(new PassField(key, label, value, textAlignment, dateStyle, numberStyle));
        return this;
    }

    /// <summary>
    /// Adds a primary field.
    /// </summary>
    /// <param name="key">The field key.</param>
    /// <param name="label">The field label.</param>
    /// <param name="value">The field value.</param>
    /// <param name="textAlignment">Optional text alignment.</param>
    /// <param name="dateStyle">Optional date style.</param>
    /// <param name="numberStyle">Optional number style.</param>
    /// <returns>The current builder.</returns>
    public PassBuilder AddPrimaryField(
        string key,
        string label,
        object value,
        PassTextAlignment? textAlignment = null,
        PassDateStyle? dateStyle = null,
        PassNumberStyle? numberStyle = null)
    {
        ActiveStyle().PrimaryFields.Add(new PassField(key, label, value, textAlignment, dateStyle, numberStyle));
        return this;
    }

    /// <summary>
    /// Adds a secondary field.
    /// </summary>
    /// <param name="key">The field key.</param>
    /// <param name="label">The field label.</param>
    /// <param name="value">The field value.</param>
    /// <param name="textAlignment">Optional text alignment.</param>
    /// <param name="dateStyle">Optional date style.</param>
    /// <param name="numberStyle">Optional number style.</param>
    /// <returns>The current builder.</returns>
    public PassBuilder AddSecondaryField(
        string key,
        string label,
        object value,
        PassTextAlignment? textAlignment = null,
        PassDateStyle? dateStyle = null,
        PassNumberStyle? numberStyle = null)
    {
        ActiveStyle().SecondaryFields.Add(new PassField(key, label, value, textAlignment, dateStyle, numberStyle));
        return this;
    }

    /// <summary>
    /// Adds an auxiliary field.
    /// </summary>
    /// <param name="key">The field key.</param>
    /// <param name="label">The field label.</param>
    /// <param name="value">The field value.</param>
    /// <param name="textAlignment">Optional text alignment.</param>
    /// <param name="dateStyle">Optional date style.</param>
    /// <param name="numberStyle">Optional number style.</param>
    /// <returns>The current builder.</returns>
    public PassBuilder AddAuxiliaryField(
        string key,
        string label,
        object value,
        PassTextAlignment? textAlignment = null,
        PassDateStyle? dateStyle = null,
        PassNumberStyle? numberStyle = null)
    {
        ActiveStyle().AuxiliaryFields.Add(new PassField(key, label, value, textAlignment, dateStyle, numberStyle));
        return this;
    }

    /// <summary>
    /// Adds a back field.
    /// </summary>
    /// <param name="key">The field key.</param>
    /// <param name="label">The field label.</param>
    /// <param name="value">The field value.</param>
    /// <param name="textAlignment">Optional text alignment.</param>
    /// <param name="dateStyle">Optional date style.</param>
    /// <param name="numberStyle">Optional number style.</param>
    /// <returns>The current builder.</returns>
    public PassBuilder AddBackField(
        string key,
        string label,
        object value,
        PassTextAlignment? textAlignment = null,
        PassDateStyle? dateStyle = null,
        PassNumberStyle? numberStyle = null)
    {
        ActiveStyle().BackFields.Add(new PassField(key, label, value, textAlignment, dateStyle, numberStyle));
        return this;
    }

    /// <summary>
    /// Adds a relevant location.
    /// </summary>
    /// <param name="latitude">The latitude.</param>
    /// <param name="longitude">The longitude.</param>
    /// <param name="altitude">The altitude.</param>
    /// <param name="relevantText">The relevant text.</param>
    /// <returns>The current builder.</returns>
    public PassBuilder AddLocation(double latitude, double longitude, double? altitude = null, string? relevantText = null)
    {
        _pass.Locations.Add(new Location(latitude, longitude, altitude, relevantText));
        return this;
    }

    /// <summary>
    /// Adds beacon metadata.
    /// </summary>
    /// <param name="proximityUuid">The proximity UUID.</param>
    /// <param name="major">The major identifier.</param>
    /// <param name="minor">The minor identifier.</param>
    /// <param name="relevantText">The relevant text.</param>
    /// <returns>The current builder.</returns>
    public PassBuilder AddBeacon(Guid proximityUuid, ushort? major = null, ushort? minor = null, string? relevantText = null)
    {
        _pass.Beacons.Add(new BeaconInfo(proximityUuid, major, minor, relevantText));
        return this;
    }

    /// <summary>
    /// Sets the relevant date.
    /// </summary>
    /// <param name="relevantDate">The relevant date.</param>
    /// <returns>The current builder.</returns>
    public PassBuilder WithRelevantDate(DateTimeOffset relevantDate)
    {
        _pass.RelevantDate = relevantDate;
        return this;
    }

    /// <summary>
    /// Builds and validates the pass.
    /// </summary>
    /// <returns>The built pass.</returns>
    public Pass Build()
    {
        PassValidator.Validate(_pass);
        return _pass;
    }

    private static string NormalizeColor(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (RgbRegex().IsMatch(value))
        {
            return value;
        }

        var hex = value.Trim();
        if (hex.StartsWith('#'))
        {
            hex = hex[1..];
        }

        if (hex.Length == 3)
        {
            hex = string.Concat(hex.Select(static c => $"{c}{c}"));
        }

        if (hex.Length != 6 || !HexRegex().IsMatch(hex))
        {
            throw new PassGenerationException(
                PassGenerationErrorCode.InvalidPassData,
                $"Invalid color format '{value}'. Use #RRGGBB, #RGB, or rgb(r, g, b).");
        }

        var r = int.Parse(hex[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var g = int.Parse(hex[2..4], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var b = int.Parse(hex[4..6], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        return $"rgb({r}, {g}, {b})";
    }

    private PassStyle ActiveStyle()
    {
        if (_activeStyle is not null)
        {
            return _activeStyle;
        }

        throw new PassGenerationException(
            PassGenerationErrorCode.InvalidPassData,
            "A pass type must be selected before adding fields.");
    }

    private void ClearStyles()
    {
        _pass.BoardingPass = null;
        _pass.EventTicket = null;
        _pass.Coupon = null;
        _pass.StoreCard = null;
        _pass.Generic = null;
    }

    [GeneratedRegex(@"^rgb\(\s*\d{1,3}\s*,\s*\d{1,3}\s*,\s*\d{1,3}\s*\)$", RegexOptions.Compiled)]
    private static partial Regex RgbRegex();

    [GeneratedRegex(@"^[0-9a-fA-F]{6}$", RegexOptions.Compiled)]
    private static partial Regex HexRegex();
}

using System.Globalization;
using System.Text.RegularExpressions;
using SPC.Infrastructure.AppleWalletPass.Models;

namespace SPC.Infrastructure.AppleWalletPass;

/// <summary>
/// Validates pass data before serialization and packaging.
/// </summary>
internal static partial class PassValidator
{
    /// <summary>
    /// Validates core pass metadata.
    /// </summary>
    /// <param name="pass">The pass to validate.</param>
    public static void Validate(Pass pass)
    {
        ArgumentNullException.ThrowIfNull(pass);

        EnsureRequired(pass.PassTypeIdentifier, nameof(pass.PassTypeIdentifier));
        EnsureRequired(pass.TeamIdentifier, nameof(pass.TeamIdentifier));
        EnsureRequired(pass.SerialNumber, nameof(pass.SerialNumber));
        EnsureRequired(pass.OrganizationName, nameof(pass.OrganizationName));
        EnsureRequired(pass.Description, nameof(pass.Description));

        if (pass.SerialNumber!.Length > 255)
        {
            throw new PassGenerationException(
                PassGenerationErrorCode.InvalidPassData,
                "The serialNumber must be 255 characters or fewer.");
        }

        if (pass.Kind is null)
        {
            throw new PassGenerationException(
                PassGenerationErrorCode.InvalidPassData,
                "A pass type must be selected. Use AsBoardingPass, AsEventTicket, AsCoupon, AsStoreCard, or AsGeneric.");
        }

        ValidateColors(pass.Colors);
    }

    /// <summary>
    /// Validates pass metadata and packaging assets.
    /// </summary>
    /// <param name="pass">The pass to validate.</param>
    public static void ValidateForGeneration(Pass pass)
    {
        Validate(pass);

        if (string.IsNullOrWhiteSpace(pass.Images.IconPath))
        {
            throw new PassGenerationException(
                PassGenerationErrorCode.ImageMissing,
                "Pass icon.png is required. Configure it with WithIcon(path).");
        }
    }

    private static void EnsureRequired(string? value, string propertyName)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        throw new PassGenerationException(
            PassGenerationErrorCode.InvalidPassData,
            $"Missing required field: {propertyName}.");
    }

    private static void ValidateColors(PassColors? colors)
    {
        if (colors is null)
        {
            return;
        }

        ValidateColor(colors.BackgroundColor, nameof(colors.BackgroundColor));
        ValidateColor(colors.ForegroundColor, nameof(colors.ForegroundColor));
        ValidateColor(colors.LabelColor, nameof(colors.LabelColor));
    }

    private static void ValidateColor(string value, string propertyName)
    {
        var match = RgbColorRegex().Match(value);
        if (!match.Success)
        {
            throw new PassGenerationException(
                PassGenerationErrorCode.InvalidPassData,
                $"Invalid color format for {propertyName}. Expected CSS rgb(r, g, b).");
        }

        for (var groupIndex = 1; groupIndex <= 3; groupIndex++)
        {
            if (int.Parse(match.Groups[groupIndex].Value, CultureInfo.InvariantCulture) > 255)
            {
                throw new PassGenerationException(
                    PassGenerationErrorCode.InvalidPassData,
                    $"Invalid color channel for {propertyName}. Each rgb component must be between 0 and 255.");
            }
        }
    }

    [GeneratedRegex(@"^rgb\(\s*(\d{1,3})\s*,\s*(\d{1,3})\s*,\s*(\d{1,3})\s*\)$", RegexOptions.Compiled)]
    private static partial Regex RgbColorRegex();
}

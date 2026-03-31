using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AppleWalletPass.Designer.Configuration;
using AppleWalletPass.Designer.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace AppleWalletPass.Designer.Services;

public sealed class WalletSigningSettingsStore(
    IWebHostEnvironment environment,
    IOptions<WalletDesignerOptions> options,
    IDataProtectionProvider dataProtectionProvider)
{
    private readonly IWebHostEnvironment _environment = environment;
    private readonly WalletDesignerOptions _options = options.Value;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector("AppleWalletPass.Designer.WalletSigningSettings.v1");

    public async Task<WalletSigningSettingsViewModel> GetAsync(CancellationToken cancellationToken)
    {
        var record = await ReadRecordAsync(cancellationToken).ConfigureAwait(false);
        if (record is null)
        {
            return new WalletSigningSettingsViewModel();
        }

        return new WalletSigningSettingsViewModel
        {
            PassTypeIdentifier = record.PassTypeIdentifier ?? string.Empty,
            TeamIdentifier = record.TeamIdentifier ?? string.Empty,
            DefaultOrganizationName = record.DefaultOrganizationName,
            HasCertificate = !string.IsNullOrWhiteSpace(record.EncryptedCertificateBase64),
            UpdatedAtUtc = record.UpdatedAtUtc
        };
    }

    public async Task SaveAsync(
        WalletSigningSettingsInput input,
        Stream? certificateStream,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        var existing = await ReadRecordAsync(cancellationToken).ConfigureAwait(false);
        string? encryptedCertificateBase64 = existing?.EncryptedCertificateBase64;

        if (certificateStream is not null)
        {
            using var memory = new MemoryStream();
            await certificateStream.CopyToAsync(memory, cancellationToken).ConfigureAwait(false);
            encryptedCertificateBase64 = _protector.Protect(Convert.ToBase64String(memory.ToArray()));
        }

        var record = new WalletSigningSettingsRecord
        {
            EncryptedCertificateBase64 = encryptedCertificateBase64,
            EncryptedCertificatePassword = _protector.Protect(input.CertificatePassword),
            PassTypeIdentifier = input.PassTypeIdentifier.Trim(),
            TeamIdentifier = input.TeamIdentifier.Trim(),
            DefaultOrganizationName = string.IsNullOrWhiteSpace(input.DefaultOrganizationName) ? null : input.DefaultOrganizationName.Trim(),
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        var path = GetSettingsPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(record, _jsonOptions), cancellationToken).ConfigureAwait(false);
    }

    public async Task<WalletSigningSettingsResolved> GetResolvedAsync(CancellationToken cancellationToken)
    {
        var record = await ReadRecordAsync(cancellationToken).ConfigureAwait(false);
        if (record is null ||
            string.IsNullOrWhiteSpace(record.EncryptedCertificateBase64) ||
            string.IsNullOrWhiteSpace(record.EncryptedCertificatePassword) ||
            string.IsNullOrWhiteSpace(record.PassTypeIdentifier) ||
            string.IsNullOrWhiteSpace(record.TeamIdentifier))
        {
            throw new InvalidOperationException("Wallet signing settings are incomplete. Open Settings and configure the certificate, pass type identifier, and team identifier.");
        }

        return new WalletSigningSettingsResolved
        {
            CertificateBytes = Convert.FromBase64String(_protector.Unprotect(record.EncryptedCertificateBase64)),
            CertificatePassword = _protector.Unprotect(record.EncryptedCertificatePassword),
            PassTypeIdentifier = record.PassTypeIdentifier,
            TeamIdentifier = record.TeamIdentifier,
            DefaultOrganizationName = record.DefaultOrganizationName,
            UpdatedAtUtc = record.UpdatedAtUtc
        };
    }

    private async Task<WalletSigningSettingsRecord?> ReadRecordAsync(CancellationToken cancellationToken)
    {
        var path = GetSettingsPath();
        if (!File.Exists(path))
        {
            return null;
        }

        var json = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<WalletSigningSettingsRecord>(json, _jsonOptions);
    }

    private string GetSettingsPath()
        => Path.GetFullPath(Path.Combine(_environment.ContentRootPath, _options.SettingsStoragePath));
}

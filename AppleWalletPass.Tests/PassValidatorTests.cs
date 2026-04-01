using SPC.Infrastructure.AppleWalletPass;
using SPC.Infrastructure.AppleWalletPass.Models;

namespace AppleWalletPass.Tests;

public class PassValidatorTests
{
    [Fact]
    public void Validate_ThrowsForMissingRequiredFields()
    {
        var pass = new Pass
        {
            Kind = PassKind.Generic,
            SerialNumber = "SER-001"
        };

        var exception = Assert.Throws<PassGenerationException>(() => PassValidator.Validate(pass));
        Assert.Equal(PassGenerationErrorCode.InvalidPassData, exception.ErrorCode);
        Assert.Contains("PassTypeIdentifier", exception.Message, StringComparison.Ordinal);
    }
}

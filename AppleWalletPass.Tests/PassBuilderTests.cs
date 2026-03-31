using System.Text.Json;
using AppleWalletPass.Models;

namespace AppleWalletPass.Tests;

public class PassBuilderTests
{
    [Fact]
    public void Build_BoardingPass_ProducesExpectedPassJsonStructure()
    {
        var pass = new PassBuilder()
            .AsBoardingPass(PKTransitType.Air)
            .WithOrganization("Acme Airlines", "ABC123XYZ", "pass.com.acme.boarding")
            .WithSerial("PKP-2024-0394-7821")
            .WithColors("#0A3D62", "#FFFFFF", "#B4C8DC")
            .AddPrimaryField("origin", "FROM", "JFK")
            .AddPrimaryField("destination", "TO", "LAX")
            .AddSecondaryField("seat", "SEAT", "14A")
            .Build();

        var json = PassJson.Serialize(pass);
        using var document = JsonDocument.Parse(json);

        Assert.Equal("pass.com.acme.boarding", document.RootElement.GetProperty("passTypeIdentifier").GetString());
        Assert.Equal("PKTransitTypeAir", document.RootElement.GetProperty("boardingPass").GetProperty("transitType").GetString());
        Assert.Equal("JFK", document.RootElement.GetProperty("boardingPass").GetProperty("primaryFields")[0].GetProperty("value").GetString());
        Assert.Equal("rgb(10, 61, 98)", document.RootElement.GetProperty("backgroundColor").GetString());
    }
}

using AppleWalletPass;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.Rendering;

namespace SPC.UI.Blazor.CRM.Services;

public sealed class BarcodePreviewRenderer
{
    public string? BuildDataUrl(DesignerBarcodeStyle style, string message)
    {
        if (style == DesignerBarcodeStyle.None || string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        var format = style switch
        {
            DesignerBarcodeStyle.QrCode => BarcodeFormat.QR_CODE,
            DesignerBarcodeStyle.Pdf417 => BarcodeFormat.PDF_417,
            DesignerBarcodeStyle.Aztec => BarcodeFormat.AZTEC,
            DesignerBarcodeStyle.Code128 => BarcodeFormat.CODE_128,
            _ => BarcodeFormat.QR_CODE
        };

        var writer = new BarcodeWriterSvg
        {
            Format = format,
            Options = BuildOptions(style)
        };

        var svg = writer.Write(message).Content;
        return $"data:image/svg+xml;base64,{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(svg))}";
    }

    private static EncodingOptions BuildOptions(DesignerBarcodeStyle style)
        => style switch
        {
            DesignerBarcodeStyle.Code128 => new EncodingOptions
            {
                Width = 260,
                Height = 80,
                Margin = 0,
                PureBarcode = true
            },
            DesignerBarcodeStyle.Pdf417 => new EncodingOptions
            {
                Width = 260,
                Height = 120,
                Margin = 0,
                PureBarcode = true
            },
            DesignerBarcodeStyle.Aztec => new EncodingOptions
            {
                Width = 180,
                Height = 180,
                Margin = 0
            },
            _ => new QrCodeEncodingOptions
            {
                Width = 180,
                Height = 180,
                Margin = 0
            }
        };
}

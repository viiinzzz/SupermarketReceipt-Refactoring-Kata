using VerifyXunit;

namespace Supermarket.Test.print;

[UsesVerify]
public class TextReceiptPrinterTest : PrinterTest
{
    public TextReceiptPrinterTest() : base(new Supermarket.Specific.TextReceiptPrinter()) { }
}
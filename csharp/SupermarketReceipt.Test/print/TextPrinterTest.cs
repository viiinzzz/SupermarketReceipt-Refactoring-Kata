using VerifyXunit;

using Supermarket.Logistics;

namespace SupermarketReceipt.Test.print;

[UsesVerify]
public class TextReceiptPrinterTest : PrinterTest
{
    public TextReceiptPrinterTest() : base(new TextReceiptPrinter()) { }
}
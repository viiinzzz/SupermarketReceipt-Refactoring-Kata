using VerifyXunit;

using Supermarket.Logistics;

namespace SupermarketReceipt.Test.print;

[UsesVerify]
public class HtmlReceiptPrinterTest : PrinterTest
{
    public HtmlReceiptPrinterTest() : base(new HtmlReceiptPrinter()) {}
}
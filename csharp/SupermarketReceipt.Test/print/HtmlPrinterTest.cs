using VerifyXunit;

namespace Supermarket.Test.print;

[UsesVerify]
public class HtmlReceiptPrinterTest : PrinterTest
{
    public HtmlReceiptPrinterTest() : base(new Supermarket.Specific.HtmlReceiptPrinter()) {}
}
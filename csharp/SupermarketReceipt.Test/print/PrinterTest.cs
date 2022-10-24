using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

using Supermarket.Core.Finance;
using Supermarket.Core.Inventory;
using Supermarket.Core.Logistics;

namespace Supermarket.Test.print;

public class PrinterTest
{
    private readonly IReceiptPrinter _printer;
    protected PrinterTest(IReceiptPrinter printer)
        => _printer = printer;

    Receipt _receipt = new Receipt();
    private VerifyTests.SettingsTask VerifyReceipt
        => Verifier.Verify(_printer.PrintReceipt(_receipt));

    private readonly Product
        _toothbrush = new Product("toothbrush", ProductUnit.Each),
        _apples = new Product("apples", ProductUnit.Kilo);

    [Fact]
    public Task T001_oneLineItem()
    {
        _receipt.AddProduct(_toothbrush, 1, 0.99, 0.99);
        return VerifyReceipt;
    }

    [Fact]
    public Task T002_quantityTwo()
    {
        _receipt.AddProduct(_toothbrush, 2, 0.99, 0.99 * 2);
        return VerifyReceipt;
    }

    [Fact]
    public Task T003_looseWeight()
    {
        _receipt.AddProduct(_apples, 2.3, 1.99, 1.99 * 2.3);
        return VerifyReceipt;
    }

    [Fact]
    public Task T004_total()
    {

        _receipt.AddProduct(_toothbrush, 1, 0.99, 2 * 0.99);
        _receipt.AddProduct(_apples, 0.75, 1.99, 1.99 * 0.75);
        return VerifyReceipt;
    }

    [Fact]
    public Task T005_discounts()
    {
        _receipt.AddDiscount(new Discount(_apples, "3 for 2", 0.99));
        return VerifyReceipt;
    }

    [Fact]
    public Task T006_printWholeReceipt()
    {
        _receipt.AddProduct(_toothbrush, 1, 0.99, 0.99);
        _receipt.AddProduct(_toothbrush, 2, 0.99, 2 * 0.99);
        _receipt.AddProduct(_apples, 0.75, 1.99, 1.99 * 0.75);
        _receipt.AddDiscount(new Discount(_toothbrush, "3 for 2", 0.99));
        return VerifyReceipt;
    }
}
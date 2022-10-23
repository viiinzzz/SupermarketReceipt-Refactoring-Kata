
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;
using NFluent;

using Supermarket.Finance;
using Supermarket.Inventory;
using Supermarket.Logistics;
using Supermarket.Marketing;
using Supermarket.Test.Inventory;

namespace Supermarket.Test;

public static class StringExtensions
{
    public static string lf(this string text) => text?.ReplaceLineEndings("\n");

    public static string ButFirstEmptyLine(this string text)
    {
        if (text == null) return null;
        var textLF = text.lf();
        var i = textLF.IndexOf('\n');
        if (i < 0) return textLF;
        if (i == 0) return textLF.Substring(i + 1);
        return textLF;
    }

    public static string lf1(this string text) => ButFirstEmptyLine(text);
}

[UsesVerify]
public class SupermarketTest
{
    private readonly Product _toothbrush, _toothpaste, _rice, _apples, _cherryTomatoes;
    private readonly ProductCatalog _catalog;
    private readonly SpecialOffers _offers;
    private readonly Cart _cart;
    

    public SupermarketTest()
    {
        _catalog = new FakeProductCatalog();
        _offers = new SpecialOffers(_catalog);
        _cart = new Cart();

        _toothbrush = new Product("toothbrush", ProductUnit.Each);
        _toothpaste = new Product("toothpaste", ProductUnit.Each);
        _rice = new Product("rice", ProductUnit.Each);
        _apples = new Product("apples", ProductUnit.Kilo);
        _cherryTomatoes = new Product("cherry tomato box", ProductUnit.Each);

        _catalog.AddProduct(_toothbrush, 0.99);
        _catalog.AddProduct(_toothpaste, 1.79);
        _catalog.AddProduct(_rice, 2.99);
        _catalog.AddProduct(_apples, 1.99);
        _catalog.AddProduct(_cherryTomatoes, 0.69);

    }

    private readonly ReceiptPrinter _receiptPrinter = new(40);

    /*
     * snapshot location
     * C:\Users\xxxx\AppData\Local\NCrunch\nnnnn\nn\SupermarketReceipt.Test\tttt.verified.txt
     *
     * not working very good if build broken
     * prefer regular check
     */
    public Task VerifyReceiptWithSnapshot
        => Verifier.Verify(receipt);

    public string receipt
        => _receiptPrinter.PrintReceipt(
            _cart.ChecksOutArticlesFrom(_offers));


    [Fact]
    public async void T001__an_empty_shopping_cart_should_cost_nothing()
    {
        // await VerifyReceiptWithSnapshot;
        Check.That(receipt.lf()).IsEqualTo(@"

Total:                              0.00
".lf1());
    }

    [Fact]
    public async void T002__one_normal_item()
    {
        _cart.AddItem(_toothbrush);

        // await VerifyReceiptWithSnapshot;
        Check.That(receipt.lf()).IsEqualTo(@"
toothbrush                          0.99

Total:                              0.99
".lf1());
    }

    [Fact]
    public async void T003__two_normal_items()
    {
        _cart.AddItem(_toothbrush);
        _cart.AddItem(_rice);

        // await VerifyReceiptWithSnapshot;
        Check.That(receipt.lf()).IsEqualTo(@"
toothbrush                          0.99
rice                                2.99

Total:                              3.98
".lf1());
    }

    [Fact]
    public async void T004__buy_two_get_one_free()
    {
        _cart.AddItem(_toothbrush);
        _cart.AddItem(_toothbrush);
        _cart.AddItem(_toothbrush);

        _offers.AddSpecialOffer(SpecialOfferType.ThreeForTwo, _toothbrush, _catalog.GetUnitPrice(_toothbrush));

        // await VerifyReceiptWithSnapshot;
        Check.That(receipt.lf()).IsEqualTo(@"
toothbrush                          0.99
toothbrush                          0.99
toothbrush                          0.99
3 for 2(toothbrush)                -0.99

Total:                              1.98
".lf1());
    }

    [Fact]
    public async void T005__buy_five_get_one_free()
    {
        _cart.AddItem(_toothbrush);
        _cart.AddItem(_toothbrush);
        _cart.AddItem(_toothbrush);
        _cart.AddItem(_toothbrush);
        _cart.AddItem(_toothbrush);

        _offers.AddSpecialOffer(SpecialOfferType.ThreeForTwo, _toothbrush, _catalog.GetUnitPrice(_toothbrush));

        // await VerifyReceiptWithSnapshot;
        Check.That(receipt.lf()).IsEqualTo(@"
toothbrush                          0.99
toothbrush                          0.99
toothbrush                          0.99
toothbrush                          0.99
toothbrush                          0.99
3 for 2(toothbrush)                -0.99

Total:                              3.96
".lf1());
    }

    [Fact]
    public async void T006__loose_weight_product()
    {
        _cart.AddItemQuantity(_apples, .5);

        // await VerifyReceiptWithSnapshot;
        Check.That(receipt.lf()).IsEqualTo(@"
apples                              0.99
  1.99 * 0.500

Total:                              0.99
".lf1());
    }

    [Fact]
    public async void T007__percent_discount()
    {
        _cart.AddItem(_rice);

        _offers.AddSpecialOffer(SpecialOfferType.PercentDiscount, _rice, 10.0);

        // await VerifyReceiptWithSnapshot;
        Check.That(receipt.lf()).IsEqualTo(@"
rice                                2.99
10% off(rice)                      -0.30

Total:                              2.69
".lf1());
    }

    [Fact]
    public async void T008__TwoForY_discount()
    {
        _cart.AddItem(_cherryTomatoes);
        _cart.AddItem(_cherryTomatoes);

        _offers.AddSpecialOffer(SpecialOfferType.TwoForAmount, _cherryTomatoes, .99);

        // await VerifyReceiptWithSnapshot;
        Check.That(receipt.lf()).IsEqualTo(@"
cherry tomato box                   0.69
cherry tomato box                   0.69
2 for 0.99(cherry tomato box)      -0.39

Total:                              0.99
".lf1());
    }

    [Fact]
    public async void T009__FiveForY_discount()
    {
        _cart.AddItemQuantity(_apples, 5);

        _offers.AddSpecialOffer(SpecialOfferType.FiveForAmount, _apples, 6.99);

        // await VerifyReceiptWithSnapshot;
        Check.That(receipt.lf()).IsEqualTo(@"
apples                              9.95
  1.99 * 5.000
5 for 6.99(apples)                 -2.96

Total:                              6.99
".lf1());
    }

    [Fact]
    public void T010__FiveForY_discount_withSix()
    {
        _cart.AddItemQuantity(_apples, 6);

        _offers.AddSpecialOffer(SpecialOfferType.FiveForAmount, _apples, 6.99);

        // await VerifyReceiptWithSnapshot;
        Check.That(receipt.lf()).IsEqualTo(@"
apples                             11.94
  1.99 * 6.000
5 for 6.99(apples)                 -2.96

Total:                              8.98
".lf1());
    }

    [Fact]
    public async void T011__FiveForY_discount_withSixteen()
    {
        _cart.AddItemQuantity(_apples, 16);

        _offers.AddSpecialOffer(SpecialOfferType.FiveForAmount, _apples, 6.99);

        // await VerifyReceiptWithSnapshot;
        Check.That(receipt.lf()).IsEqualTo(@"
apples                             31.84
  1.99 * 16.000
5 for 6.99(apples)                 -8.88

Total:                             22.96
".lf1());
    }

    [Fact]
    public async void T012__FiveForY_discount_withFour()
    {
        _cart.AddItemQuantity(_apples, 4);

        _offers.AddSpecialOffer(SpecialOfferType.FiveForAmount, _apples, 6.99);

        // await VerifyReceiptWithSnapshot;
        Check.That(receipt.lf()).IsEqualTo(@"
apples                              7.96
  1.99 * 4.000

Total:                              7.96
".lf1());
    }

    [Fact]
    public async void T013__nobundle_discount()
    {
        _cart.AddItem(_toothbrush);
        _cart.AddItem(_toothpaste);
        
        // await VerifyReceiptWithSnapshot;
        Check.That(receipt.lf()).IsEqualTo(@"
toothbrush                          0.99
toothpaste                          1.79

Total:                              2.78
".lf1());
    }

    [Fact]
    public async void T014__bundle_discount()
    {
        _cart.AddItem(_toothbrush);
        _cart.AddItem(_toothpaste);

        var bundle = new[]
        {
            (_toothbrush, 1),
            (_toothpaste, 1)
        };
        _offers.AddSpecialOffer(SpecialOfferType.PercentDiscount, bundle, 10);

        // await VerifyReceiptWithSnapshot;
        Check.That(receipt.lf()).IsEqualTo(@"
toothbrush                          0.99
toothpaste                          1.79
10% off(bundle)                    -0.28

Total:                              2.50
".lf1());
    }
}
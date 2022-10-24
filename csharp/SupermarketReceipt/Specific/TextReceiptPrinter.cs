using System.Globalization;
using System.Linq;

using Supermarket.Core.Finance;
using Supermarket.Core.Inventory;
using Supermarket.Core.Logistics;

namespace Supermarket.Specific;

public class TextReceiptPrinter : IReceiptPrinter
{
    private static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("en-GB");

    private readonly int _columns;


    public TextReceiptPrinter(int columns) => _columns = columns;
    public TextReceiptPrinter() : this(40) { }

    public string PrintReceipt(Receipt receipt)
        => receipt.GetItems()
               .Select(item => PrintReceiptItem(item))
               .Aggregate("", (x, y) => x + y)
           + receipt.GetDiscounts()
               .Select(discount => PrintDiscount(discount))
               .Aggregate("", (x, y) => x + y)
           + "\n"
           + PrintTotal(receipt);

    public string PrintTotal(Receipt receipt)
        => FormatLineWithWhitespace(
            "Total: ",
            PrintPrice(receipt.GetTotalPrice()));

    public string PrintDiscount(Discount discount)
        => FormatLineWithWhitespace(
            $"{discount.Description}({discount.Product.Name})",
            PrintPrice(discount.DiscountAmount));

    public string PrintReceiptItem(ReceiptItem item)
        => FormatLineWithWhitespace(
               item.Product.Name,
               PrintPrice(item.TotalPrice))
            + (item.Quantity != 1 ? $"  {PrintPrice(item.Price)} * {PrintQuantity(item)}\n" : "");

    private string FormatLineWithWhitespace(string name, string value)
        => $"{name}{new string(' ', _columns - name.Length - value.Length)}{value}\n";

    public string PrintPrice(double price)
        => price.ToString("N2", Culture);

    private static string PrintQuantity(ReceiptItem item)
        => ProductUnit.Each == item.Product.Unit
            ? ((int)item.Quantity).ToString()
            : item.Quantity.ToString("N3", Culture);
}
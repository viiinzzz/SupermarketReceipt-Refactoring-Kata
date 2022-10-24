using System.Globalization;
using System.Linq;
using Supermarket.Core.Finance;
using Supermarket.Core.Inventory;
using Supermarket.Core.Logistics;

namespace Supermarket.Specific;

public class HtmlReceiptPrinter : IReceiptPrinter
{
    private static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("en-GB");


    public string PrintReceipt(Receipt receipt)
        => @"
<html>
  <body>
    <table>"
           + receipt.GetItems()
                .Select(item => PrintReceiptItem(item))
                .Aggregate("", (x, y) => x + y)
           + receipt.GetDiscounts()
               .Select(discount => PrintDiscount(discount))
               .Aggregate("", (x, y) => x + y)
           + PrintTotal(receipt)
           + @"
  </body>
</html>
";

    public string PrintTotal(Receipt receipt)
        => @$"
      <tr>
        <td colspan=""2"">
        </td>
      </tr>
      <tr>
        <td colspan=""2"">
          Total: {PrintPrice(receipt.GetTotalPrice())}
        </td>
      </tr>";

    public string PrintDiscount(Discount discount)
        => @$"
      <tr>
        <td>
          {discount.Description}({discount.Product.Name})
        </td>
        <td>
          {PrintPrice(discount.DiscountAmount)}
        </td>
      </tr>";

    public string PrintReceiptItem(ReceiptItem item)
        => @$"
      <tr>
        <td>
          {item.Product.Name}
        </td>
        <td>
          {PrintPrice(item.TotalPrice)}
        </td>
      </tr>" + (item.Quantity == 1 ? "" : @$"
      <tr>
        <td colspan=""2"">
          {PrintPrice(item.Price)} * {PrintQuantity(item)}
        </td>
      </tr>");

    public string PrintPrice(double price)
        => price.ToString("N2", Culture);

    private static string PrintQuantity(ReceiptItem item)
        => ProductUnit.Each == item.Product.Unit
            ? ((int)item.Quantity).ToString()
            : item.Quantity.ToString("N3", Culture);
}
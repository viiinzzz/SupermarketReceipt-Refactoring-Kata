using Supermarket.Core.Inventory;

namespace Supermarket.Core.Finance;

public class ReceiptItem
{
    public ReceiptItem(Product p, double quantity, double price, double totalPrice)
    {
        Product = p;
        Quantity = quantity;
        Price = price;
        TotalPrice = totalPrice;
    }

    public Product Product { get; }
    public double Price { get; }
    public double TotalPrice { get; }
    public double Quantity { get; }
}
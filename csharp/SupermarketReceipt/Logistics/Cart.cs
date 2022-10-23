using System.Collections.Generic;

using Supermarket.Finance;
using Supermarket.Inventory;
using Supermarket.Marketing;

namespace Supermarket.Logistics;

public class Cart
{
    private readonly List<ProductQuantity> _items = new();
    private readonly Dictionary<Product, double> _productQuantities = new();

    public ProductQuantity[] GetItems() => _items.ToArray();
    public Dictionary<Product, double> GetMap() => new(_productQuantities);

    public void AddItem(Product product) => AddItemQuantity(product, 1.0);


    public void AddItemQuantity(Product product, double quantity)
    {
        _items.Add(new ProductQuantity(product, quantity));
        if (_productQuantities.ContainsKey(product))
        {
            var newAmount = _productQuantities[product] + quantity;
            _productQuantities[product] = newAmount;
        }
        else
        {
            _productQuantities.Add(product, quantity);
        }
    }

    public Receipt ChecksOutArticlesFrom(SpecialOffers specialOffers)
    {
        var receipt = new Receipt();
        var productQuantities = GetItems();
        foreach (var pq in productQuantities)
        {
            var p = pq.Product;
            var quantity = pq.Quantity;

            var unitPrice = specialOffers.Catalog.GetUnitPrice(p);
            var price = quantity * unitPrice;

            receipt.AddProduct(p, quantity, unitPrice, price);
        }

        receipt.HandleOffers(specialOffers, specialOffers.Catalog, this);

        return receipt;
    }
}
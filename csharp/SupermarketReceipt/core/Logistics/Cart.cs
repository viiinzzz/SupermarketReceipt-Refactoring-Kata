using System;
using System.Collections.Generic;

using Supermarket.Core.Finance;
using Supermarket.Core.Inventory;
using Supermarket.Core.Marketing;

namespace Supermarket.Core.Logistics;

public class Cart
{
    private readonly List<ProductQuantity> _items = new();
    private readonly Dictionary<Product, double> _productQuantities = new();

    public ProductQuantity[] GetItems() => _items.ToArray();
    public Dictionary<Product, double> GetMap() => new(_productQuantities);
    

    public void AddItem(Product product, double quantity = 1.0)
    {
        _items.Add(new ProductQuantity(product, quantity));
        if (_productQuantities.ContainsKey(product))
            _productQuantities[product] += quantity;
        else _productQuantities.Add(product, quantity);
    }

    public void RemoveItem(Product product, double quantity = 1.0)
    {
        if (_productQuantities.ContainsKey(product))
            _productQuantities[product] -= quantity;
        else throw new Exception("cannot remove quantity: not enough available");
    }

    public Receipt ChecksOutArticlesFrom(SpecialOffers specialOffers)
    {
        var receipt = new Receipt();
        foreach (var (product, quantity) in _items)
        {
            var unitPrice = specialOffers.Catalog.GetUnitPrice(product);
            var price = quantity * unitPrice;

            receipt.AddProduct(product, quantity, unitPrice, price);
        }

        receipt.HandleOffers(specialOffers, specialOffers.Catalog, this);

        return receipt;
    }
}
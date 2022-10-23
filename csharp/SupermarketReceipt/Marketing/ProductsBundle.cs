using System;
using System.Collections.Generic;
using System.Linq;

using Supermarket.Inventory;

namespace Supermarket.Marketing;

public record class ProductsBundleItem(Product product, double quantity);

public class ProductsBundle
{
    private readonly ProductsBundleItem[] _items;

    public ProductsBundle(IEnumerable<(Product product, double quantity)> items)
    {
        _items = items
            .Select(bundleItem => new ProductsBundleItem(bundleItem.product, bundleItem.quantity))
            .ToArray();
    }
    public ProductsBundle(IEnumerable<(Product product, int quantity)> items)
        : this(items.Select(item => (item.product, (double)item.quantity))) {}

    public bool HasProduct(Product product) => _items.Any(bundleItem => bundleItem.product.Equals(product));

    public List<ProductQuantity> ApplyToProductQuantities(Dictionary<Product, double> productQuantities)
    {
        var productBundles = _items
            .Select(bundleItem => (
                bundleItem.product,
                bundleItem.quantity,
                bundleCount: Math.Floor(
                    productQuantities.GetValueOrDefault(bundleItem.product, 0)
                    / bundleItem.quantity)
            ));
        var AtLeastOneBundle = productBundles
            .All(p => p.bundleCount >= 1);
        if (!AtLeastOneBundle) return new List<ProductQuantity>();
        return productBundles
            .Select(p => new ProductQuantity(p.product, p.bundleCount * p.quantity))
            .ToList();
    }
}
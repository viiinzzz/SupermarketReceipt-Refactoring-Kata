using System.Collections.Generic;
using System.Linq;
using Supermarket.Inventory;

namespace Supermarket.Marketing;

public class SpecialOffers
{
    private readonly Dictionary<Product, SpecialOffer> _singleProductOffers = new();
    private readonly List<SpecialOffer> _productsBundleOffers = new();
    public ProductCatalog Catalog { get; private set; }

    public SpecialOffers(ProductCatalog catalog)
        => Catalog = catalog;

    public SpecialOffer GetSingleProductOffer(Product product)
        => _singleProductOffers.ContainsKey(product) ? _singleProductOffers[product] : null;
    public IEnumerable<SpecialOffer> GetProductsBundleOffers()
        => _productsBundleOffers.ToArray();

    public void AddSpecialOffer(
        SpecialOfferType offerType,
        Product product,
        double argument)
        => _singleProductOffers[product] = new SpecialOffer(offerType, product, argument);

    public void AddSpecialOffer(
        SpecialOfferType offerType,
        IEnumerable<(Product product, int quantity)> bundle,
        double argument)
        => _productsBundleOffers.Add(new SpecialOffer(offerType, new ProductsBundle(bundle), argument));

    public void AddSpecialOffer(
        SpecialOfferType offerType,
        IEnumerable<(Product product, double quantity)> bundle,
        double argument)
        => _productsBundleOffers.Add(new SpecialOffer(offerType, new ProductsBundle(bundle), argument));
}
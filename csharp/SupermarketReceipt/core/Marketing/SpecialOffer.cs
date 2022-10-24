using Supermarket.Core.Inventory;
using System.Collections.Generic;

namespace Supermarket.Core.Marketing;

public class SpecialOffer
{
    private Product _product;
    private ProductsBundle _bundle;

    public SpecialOffer(SpecialOfferType offerType, Product product, double argument)
    {
        OfferType = offerType;
        Argument = argument;
        _product = product;
        _bundle = null;
    }

    public SpecialOffer(SpecialOfferType offerType, ProductsBundle bundle, double argument)
    {
        OfferType = offerType;
        Argument = argument;
        _product = null;
        _bundle = bundle;
    }

    public SpecialOfferType OfferType { get; }
    public double Argument { get; }

    public List<ProductQuantity> ApplyBundle(Dictionary<Product, double> productQuantities)
        => _bundle?.Apply(productQuantities);
}
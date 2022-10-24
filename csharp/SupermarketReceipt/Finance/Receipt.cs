using System;
using System.Collections.Generic;
using System.Linq;

using Supermarket.Inventory;
using Supermarket.Logistics;
using Supermarket.Marketing;

namespace Supermarket.Finance;

public class Receipt
{
    private readonly List<Discount> _discounts = new();
    private readonly List<ReceiptItem> _items = new();

    public double GetTotalPrice()
        => _items.Select(item => item.TotalPrice).Sum()
           +_discounts.Select(discount => discount.DiscountAmount).Sum();

    public void AddProduct(Product p, double quantity, double price, double totalPrice)
        => _items.Add(new ReceiptItem(p, quantity, price, totalPrice));

    public List<ReceiptItem> GetItems()
        => new(_items);

    public void AddDiscount(Discount discount)
        => _discounts.Add(discount);

    public List<Discount> GetDiscounts()
        => new(_discounts);

    public void HandleOffers(SpecialOffers offers, ProductCatalog catalog, Cart cart)
    {
        foreach (var offer in offers.GetProductsBundleOffers())
        {
            var bundle = offer.ApplyBundle(cart.GetMap());
            if (bundle == null) continue;
            foreach (var item in bundle)
            {
                var unitPrice = catalog.GetUnitPrice(item.product);
                AddDiscount(item.product, item.weight, unitPrice, offer, true);
                cart.RemoveItem(item.product, item.weight);
            }
        }

        foreach (var (product, quantity) in cart.GetMap())
        {
            var unitPrice = catalog.GetUnitPrice(product);
            AddDiscount(product, quantity, unitPrice, offers);
        }
    }

    private void AddDiscount(Product product, double quantity, double unitPrice, SpecialOffers offers)
    {
        var offer = offers.GetSingleProductOffer(product);
        if (offer == null) return;
        AddDiscount(product, quantity, unitPrice, offer, false);
    }

    private void AddDiscount(Product product, double quantity, double unitPrice, SpecialOffer offer, bool bundle)
    {
        var isPercentOffer = offer.OfferType == SpecialOfferType.PercentDiscount;
        if (isPercentOffer)
        {
            var (description, discountAmount) = getDiscountPercent(
                quantity, unitPrice, offer.Argument, bundle);
            this.AddDiscount(new Discount(product, description, discountAmount));
            return;
        }

        var (offerQuantity, paidQuantity) = offer.OfferType switch
        {
            SpecialOfferType.TwoForAmount => (2, 0),
            SpecialOfferType.FiveForAmount => (5, 0),
            SpecialOfferType.ThreeForTwo => (3, 2),
            _ => throw new Exception("unsupported offer")
        };

        var quantityAsInt = (int)quantity;
        var offerTimesApplied = quantityAsInt / offerQuantity;

        if (offerTimesApplied <= 0) return;

        var isAmountOffer = offerQuantity > 0 && paidQuantity == 0;
        var isQuantityOffer = paidQuantity > 0 && paidQuantity > 0;
        if (!(isAmountOffer || isQuantityOffer)) return;

        {
            var (description, discountAmount) = getDiscountQuantity(
                quantity, quantityAsInt,
                unitPrice, offer.Argument,
                offerQuantity, paidQuantity, offerTimesApplied,
                isQuantityOffer, isAmountOffer,
                bundle);
            this.AddDiscount(new Discount(product, description, discountAmount));
            return;
        }
    }

    private static (string description, double discountAmount) getDiscountPercent(
        double quantity,
        double unitPrice,
        double offerArgument,

        bool bundle
    )
    {
        var catalogPrice = quantity * unitPrice;
        var offerPercent = offerArgument / 100.0;
        var discountAmount = -(catalogPrice * offerPercent);
        var description = offerArgument + "% off" + (bundle ? " bundle" : "");
        return (description, discountAmount);
    }

    private static (string description, double discountAmount) getDiscountQuantity(
        double quantity,
        int quantityAsInt,
        double unitPrice,
        double offerArgument,

        int offerQuantity,
        int paidQuantity,
        int offerTimesApplied,

        bool isQuantityOffer,
        bool isAmountOffer,

        bool bundle
    )
    {
        var noDiscountQuantity = quantityAsInt % offerQuantity;
        var noDiscountPrice = noDiscountQuantity * unitPrice;
        var catalogPrice = quantity * unitPrice;

        double offerPrice;
        string description;
        if (isQuantityOffer)
        {
            offerPrice = paidQuantity * unitPrice;
            description = offerQuantity + " for " + paidQuantity + (bundle ? " bundle" : "");
        }
        else if (isAmountOffer)
        {
            offerPrice = offerArgument;
            description = offerQuantity + " for " + offerArgument + (bundle ? " bundle" : "");
        }
        else throw new Exception();

        var discountPrice = offerTimesApplied * offerPrice;
        var totalPrice = discountPrice + noDiscountPrice;
        var discountAmount = -(catalogPrice - totalPrice);
        return (description, discountAmount);
    }


}
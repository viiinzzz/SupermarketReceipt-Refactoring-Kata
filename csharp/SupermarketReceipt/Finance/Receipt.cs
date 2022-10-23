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
        foreach (var (product, quantity) in cart.GetMap())
        {
            var offer = offers.GetSingleProductOffer(product);
            if (offer == null) continue;

            var unitPrice = catalog.GetUnitPrice(product);

            var isPercentOffer = offer.OfferType == SpecialOfferType.PercentDiscount;
            if (isPercentOffer)
            {
                var (description, discountAmount) = getDiscountPercent(
                    quantity, unitPrice, offer.Argument);
                this.AddDiscount(new Discount(product, description, discountAmount));
                continue;
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

            if (offerTimesApplied <= 0) continue;

            var isAmountOffer = offerQuantity > 0 && paidQuantity == 0;
            var isQuantityOffer = paidQuantity > 0 && paidQuantity > 0;
            if (!(isAmountOffer || isQuantityOffer)) continue;

            {
                var (description, discountAmount) = getDiscountQuantity(
                    quantity, quantityAsInt,
                    unitPrice, offer.Argument,
                    offerQuantity, paidQuantity, offerTimesApplied,
                    isQuantityOffer, isAmountOffer);
                this.AddDiscount(new Discount(product, description, discountAmount));
                continue;
            }
        }
    }

    private static (string description, double discountAmount) getDiscountPercent(
        double quantity,
        double unitPrice,
        double offerArgument
    )
    {
        var catalogPrice = quantity * unitPrice;
        var offerPercent = offerArgument / 100.0;
        var discountAmount = -(catalogPrice * offerPercent);
        var description = offerArgument + "% off";
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
        bool isAmountOffer
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
            description = offerQuantity + " for " + paidQuantity;
        }
        else if (isAmountOffer)
        {
            offerPrice = offerArgument;
            description = offerQuantity + " for " + offerArgument;
        }
        else throw new Exception();

        var discountPrice = offerTimesApplied * offerPrice;
        var totalPrice = discountPrice + noDiscountPrice;
        var discountAmount = -(catalogPrice - totalPrice);
        return (description, discountAmount);
    }


}
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SupermarketReceipt
{
    public class ShoppingCart
    {
        private readonly List<ProductQuantity> _items = new List<ProductQuantity>();
        private readonly Dictionary<Product, double> _productQuantities = new Dictionary<Product, double>();


        public List<ProductQuantity> GetItems() => new List<ProductQuantity>(_items);

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

        public void HandleOffers(Receipt receipt, Dictionary<Product, Offer> offers, SupermarketCatalog catalog)
        {
            foreach (var (product, quantity) in _productQuantities)
            {
                if (!offers.TryGetValue(product, out var offer)) continue;

                var unitPrice = catalog.GetUnitPrice(product);

                var isPercentOffer = offer.OfferType == SpecialOfferType.PercentDiscount;
                if (isPercentOffer)
                {
                    var(description, discountAmount) = getDiscountPercent(
                        quantity, unitPrice, offer.Argument);
                    receipt.AddDiscount(new Discount(product, description, discountAmount));
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
                    receipt.AddDiscount(new Discount(product, description, discountAmount));
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
}
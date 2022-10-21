using System.Collections.Generic;

namespace SupermarketReceipt
{
    public class ShoppingCart
    {
        private readonly List<ProductQuantity> _items = new List<ProductQuantity>();
        private readonly Dictionary<Product, double> _productQuantities = new Dictionary<Product, double>();


        public List<ProductQuantity> GetItems()
        {
            return new List<ProductQuantity>(_items);
        }

        public void AddItem(Product product)
        {
            AddItemQuantity(product, 1.0);
        }


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
            foreach (var (p, quantity) in _productQuantities)
            {
                var quantityAsInt = (int) quantity;
                if (!offers.ContainsKey(p)) continue;

                var offer = offers[p];
                var unitPrice = catalog.GetUnitPrice(p);
                Discount discount = null;
                var offerQuantity = 1;
                switch (offer.OfferType)
                {
                    case SpecialOfferType.ThreeForTwo:
                        offerQuantity = 3;
                        break;

                    case SpecialOfferType.TwoForAmount:
                        offerQuantity = 2;
                        if (quantityAsInt >= 2)
                        {
                            var total = offer.Argument * (quantityAsInt / x) + quantityAsInt % 2 * unitPrice;
                            var discountN = unitPrice * quantity - total;
                            discount = new Discount(p, "2 for " + offer.Argument, -discountN);
                        }
                        break;

                    case SpecialOfferType.FiveForAmount:
                        offerQuantity = 5;
                        break;
                }

                var offerTimesApplied = quantityAsInt / x;
                switch (offer.OfferType)
                {
                    case SpecialOfferType.ThreeForTwo when quantityAsInt > 2:
                        var discountAmount = quantity * unitPrice - (offerTimesApplied * 2 * unitPrice + quantityAsInt % 3 * unitPrice);
                        discount = new Discount(p, "3 for 2", -discountAmount);
                        break;

                    case SpecialOfferType.TenPercentDiscount:
                        discount = new Discount(p, offer.Argument + "% off", -quantity * unitPrice * offer.Argument / 100.0);
                        break;

                    case SpecialOfferType.FiveForAmount when quantityAsInt >= 5:
                        var discountTotal = unitPrice * quantity - (offer.Argument * offerTimesApplied + quantityAsInt % 5 * unitPrice);
                        discount = new Discount(p, x + " for " + offer.Argument, -discountTotal);
                        break;
                }

                if (discount != null)
                    receipt.AddDiscount(discount);
            }
        }
    }
}
using Supermarket.Finance;

namespace Supermarket.Logistics;

public interface IReceiptPrinter
{
    string PrintReceipt(Receipt receipt);
    string PrintTotal(Receipt receipt);
    string PrintDiscount(Discount discount);
    string PrintReceiptItem(ReceiptItem item);
    string PrintPrice(double price);
}
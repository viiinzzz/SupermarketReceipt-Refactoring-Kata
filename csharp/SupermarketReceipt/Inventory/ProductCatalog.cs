namespace Supermarket.Inventory;

public interface ProductCatalog
{
    void AddProduct(Product product, double price);

    double GetUnitPrice(Product product);
}
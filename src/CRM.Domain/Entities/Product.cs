namespace CRM.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Sku { get; private set; }
    public string Name { get; private set; }
    public decimal UnitPrice { get; private set; }
    public bool IsActive { get; private set; }

    public Product(string sku, string name, decimal unitPrice, bool isActive)
    {
        Sku = sku;
        Name = name;
        UnitPrice = unitPrice;
        IsActive = isActive;
    }

    public void Update(string sku, string name, decimal unitPrice, bool isActive)
    {
        Sku = sku;
        Name = name;
        UnitPrice = unitPrice;
        IsActive = isActive;
    }
}

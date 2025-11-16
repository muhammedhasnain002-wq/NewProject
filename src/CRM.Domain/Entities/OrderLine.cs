namespace CRM.Domain.Entities;

public class OrderLine
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal { get; private set; }

    public Order Order { get; private set; } = null!;
    public Product Product { get; private set; } = null!;

    public OrderLine(Guid orderId, Guid productId, int quantity, decimal unitPrice)
    {
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        LineTotal = quantity * unitPrice;
    }
}

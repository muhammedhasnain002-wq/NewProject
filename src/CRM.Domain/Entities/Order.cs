using CRM.Domain.Enums;

namespace CRM.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid CustomerId { get; private set; }
    public DateTime OrderDate { get; private set; } = DateTime.UtcNow;
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public decimal Total { get; private set; }

    public ICollection<OrderLine> Lines { get; private set; } = new List<OrderLine>();

    public Customer Customer { get; private set; } = null!;

    public Order(Guid customerId)
    {
        CustomerId = customerId;
    }

    public void AddLine(Guid productId, int quantity, decimal unitPrice)
    {
        var line = new OrderLine(Id, productId, quantity, unitPrice);
        Lines.Add(line);
        Total += line.LineTotal;
    }

    public void SetStatus(OrderStatus status) => Status = status;
}

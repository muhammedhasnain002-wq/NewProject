namespace CRM.Domain.Entities;

public class OpportunityProduct
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OpportunityId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    public Opportunity Opportunity { get; private set; } = null!;
    public Product Product { get; private set; } = null!;

    public OpportunityProduct(Guid opportunityId, Guid productId, int quantity, decimal unitPrice)
    {
        OpportunityId = opportunityId;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}

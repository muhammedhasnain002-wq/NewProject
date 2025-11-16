using CRM.Domain.Enums;

namespace CRM.Domain.Entities;

public class Opportunity
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid CustomerId { get; private set; }
    public string Title { get; private set; }
    public decimal Amount { get; private set; }
    public OpportunityStage Stage { get; private set; }
    public DateTime CloseDate { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public Customer Customer { get; private set; } = null!;
    public ICollection<Activity> Activities { get; private set; } = new List<Activity>();

    public Opportunity(Guid customerId, string title, decimal amount, OpportunityStage stage, DateTime closeDate)
    {
        CustomerId = customerId;
        Title = title;
        Amount = amount;
        Stage = stage;
        CloseDate = closeDate;
    }

    public void SetStage(OpportunityStage stage) => Stage = stage;
}

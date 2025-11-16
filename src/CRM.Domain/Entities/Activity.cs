using CRM.Domain.Enums;

namespace CRM.Domain.Entities;

public class Activity
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OpportunityId { get; private set; }
    public ActivityType Type { get; private set; }
    public string Subject { get; private set; }
    public string? Notes { get; private set; }
    public DateTime DueDate { get; private set; }
    public bool Completed { get; private set; }

    public Opportunity Opportunity { get; private set; } = null!;

    public Activity(Guid opportunityId, ActivityType type, string subject, string? notes, DateTime dueDate, bool completed)
    {
        OpportunityId = opportunityId;
        Type = type;
        Subject = subject;
        Notes = notes;
        DueDate = dueDate;
        Completed = completed;
    }
}

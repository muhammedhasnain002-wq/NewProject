using CRM.Domain.Enums;

namespace CRM.Application.DTOs;

public sealed record CreateOpportunityRequest(Guid CustomerId, string Title, decimal Amount, OpportunityStage Stage, DateTime CloseDate);
public sealed record UpdateOpportunityStageRequest(OpportunityStage Stage);
public sealed record OpportunityDto(Guid Id, Guid CustomerId, string Title, decimal Amount, OpportunityStage Stage, DateTime CloseDate, DateTime CreatedAt);
public sealed record ActivityDto(Guid Id, Guid OpportunityId, ActivityType Type, string Subject, string? Notes, DateTime DueDate, bool Completed);

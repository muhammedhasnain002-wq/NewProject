using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Abstractions.Repositories;

public interface IOpportunityRepository
{
    Task AddAsync(Opportunity opportunity);
    Task<Opportunity?> GetByIdAsync(Guid id);
    DbSet<Opportunity> Query();
}

using CRM.Application.Abstractions.Repositories;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Repositories;

public class OpportunityRepository : IOpportunityRepository
{
    private readonly Data.CrmDbContext _db;

    private static readonly Func<Data.CrmDbContext, Guid, Task<Opportunity?>> _getByIdCompiled =
        EF.CompileAsyncQuery((Data.CrmDbContext ctx, Guid id) => ctx.Opportunities.Include(o => o.Activities).FirstOrDefault(o => o.Id == id));

    public OpportunityRepository(Data.CrmDbContext db) { _db = db; }

    public Task<Opportunity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => _getByIdCompiled(_db, id);

    public async Task AddAsync(Opportunity opportunity, CancellationToken cancellationToken = default) => await _db.Opportunities.AddAsync(opportunity, cancellationToken);

    public void Update(Opportunity opportunity) => _db.Opportunities.Update(opportunity);

    public void Remove(Opportunity opportunity) => _db.Opportunities.Remove(opportunity);

    public IQueryable<Opportunity> Query() => _db.Opportunities.AsQueryable();
}

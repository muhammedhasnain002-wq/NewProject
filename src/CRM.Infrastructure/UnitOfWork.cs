using CRM.Application.Abstractions;
using CRM.Infrastructure.Data;

namespace CRM.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly CrmDbContext _db;
    public UnitOfWork(CrmDbContext db) { _db = db; }
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _db.SaveChangesAsync(cancellationToken);
}

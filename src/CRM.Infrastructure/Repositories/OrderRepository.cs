using CRM.Application.Abstractions.Repositories;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly Data.CrmDbContext _db;

    public OrderRepository(Data.CrmDbContext db) { _db = db; }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _db.Orders.Include(o => o.Lines).FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default) => await _db.Orders.AddAsync(order, cancellationToken);

    public void Update(Order order) => _db.Orders.Update(order);

    public IQueryable<Order> Query() => _db.Orders.AsQueryable();
}

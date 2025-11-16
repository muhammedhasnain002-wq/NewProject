using CRM.Application.Abstractions.Repositories;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly Data.CrmDbContext _db;

    private static readonly Func<Data.CrmDbContext, Guid, Task<Customer?>> _getByIdCompiled =
        EF.CompileAsyncQuery((Data.CrmDbContext ctx, Guid id) => ctx.Customers.Include(c => c.Contacts).FirstOrDefault(c => c.Id == id));

    private static readonly Func<Data.CrmDbContext, string, Task<bool>> _emailExistsCompiled =
        EF.CompileAsyncQuery((Data.CrmDbContext ctx, string email) => ctx.Customers.AsNoTracking().Any(c => c.Email == email));

    public CustomerRepository(Data.CrmDbContext db) { _db = db; }

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => _getByIdCompiled(_db, id);

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) => _emailExistsCompiled(_db, email);

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default) => await _db.Customers.AddAsync(customer, cancellationToken);

    public void Update(Customer customer) => _db.Customers.Update(customer);

    public void Remove(Customer customer) => _db.Customers.Remove(customer);

    public IQueryable<Customer> Query() => _db.Customers.AsQueryable();
}

using CRM.Application.Abstractions.Repositories;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly Data.CrmDbContext _db;

    private static readonly Func<Data.CrmDbContext, string, Task<Product?>> _getBySkuCompiled =
        EF.CompileAsyncQuery((Data.CrmDbContext ctx, string sku) => ctx.Products.AsNoTracking().FirstOrDefault(p => p.Sku == sku));

    public ProductRepository(Data.CrmDbContext db) { _db = db; }

    public Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default) => _getBySkuCompiled(_db, sku);

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default) => await _db.Products.AddAsync(product, cancellationToken);

    public IQueryable<Product> Query() => _db.Products.AsQueryable();
}

using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Abstractions.Repositories;

public interface IProductRepository
{
    Task AddAsync(Product product);
    Task<Product?> GetByIdAsync(Guid id);
    Task<Product?> GetBySkuAsync(string sku);
    DbSet<Product> Query();
}

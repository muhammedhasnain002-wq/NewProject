using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Abstractions.Repositories;

public interface IOrderRepository
{
    Task AddAsync(Order order);
    Task<Order?> GetByIdAsync(Guid id);
    DbSet<Order> Query();
}

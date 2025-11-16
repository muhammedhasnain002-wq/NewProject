using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Abstractions.Repositories;

public interface ICustomerRepository
{
    Task AddAsync(Customer customer);
    Task<Customer?> GetByIdAsync(Guid id);
    Task<bool> EmailExistsAsync(string email);
    DbSet<Customer> Query();
    void Remove(Customer customer);
}

using System.Threading.Tasks;

namespace CRM.Application.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();
}

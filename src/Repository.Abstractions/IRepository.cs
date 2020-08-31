using System;
using System.Threading;
using System.Threading.Tasks;

namespace Foralla.KISS.Repository
{
    public interface IRepository
    {
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
    }
}

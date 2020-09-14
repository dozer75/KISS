using System.Threading;
using System.Threading.Tasks;

namespace Foralla.KISS.Repository
{
    public interface ITransaction
    {
        void Commit();

        Task CommitAsync(CancellationToken cancellationToken = default);
    }
}

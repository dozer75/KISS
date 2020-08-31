using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Foralla.KISS.Repository
{
    internal class EFTransaction : ITransaction
    {
        private readonly TransactionScope _transaction;

        public EFTransaction(TransactionScope transaction)
        {
            _transaction = transaction;
        }

        public void Commit()
        {
            _transaction.Complete();
        }

        public Task CommitAsync(CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                _transaction.Complete();
            }

            return Task.CompletedTask;
        }
    }
}

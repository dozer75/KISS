using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Foralla.KISS.Repository
{
    [ExcludeFromCodeCoverage]
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

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken != default)
            {
                throw new ArgumentException($"{nameof(cancellationToken)} is not supported with EF repository.", nameof(cancellationToken));
            }

            Commit();

            return Task.CompletedTask;
        }
    }
}

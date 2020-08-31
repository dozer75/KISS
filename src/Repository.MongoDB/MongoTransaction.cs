using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Foralla.KISS.Repository
{
    internal class MongoTransaction : ITransaction
    {
        private readonly IClientSessionHandle _clientSession;

        public MongoTransaction(IClientSessionHandle clientSession, MongoOptions options)
        {
            _clientSession = clientSession ?? throw new ArgumentNullException(nameof(clientSession));

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (!_clientSession.IsInTransaction)
            {
                _clientSession.StartTransaction(options.TransactionOptions);
            }
        }

        public void Commit()
        {
            if (_clientSession.IsInTransaction)
            {
                _clientSession.CommitTransaction();
            }
        }

        public async Task CommitAsync(CancellationToken cancellationToken)
        {
            if (_clientSession.IsInTransaction)
            {
                await _clientSession.CommitTransactionAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}

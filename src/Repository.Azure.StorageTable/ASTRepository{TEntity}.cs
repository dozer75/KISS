using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Foralla.KISS.Repository.Wrappers;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Pluralize.NET;

namespace Foralla.KISS.Repository
{
    public abstract class ASTRepository<TEntity> : IASTRepository<TEntity>
        where TEntity : ASTEntityBase, new()
    {
        private readonly ILogger<ASTRepository<TEntity>> _logger;
        private readonly string _tableName;
        private readonly CloudTable _table;
        private readonly IQueryable<TEntity> _queryable;

        public Type ElementType => _queryable.ElementType;

        public Expression Expression => _queryable.Expression;

        public IQueryProvider Provider => _queryable.Provider;

        protected ASTRepository(CloudTableClient tableClient, IPluralize pluralize, ILogger<ASTRepository<TEntity>> logger)
        {
            if (tableClient is null)
            {
                throw new ArgumentNullException(nameof(tableClient));
            }

            if (pluralize is null)
            {
                throw new ArgumentNullException(nameof(pluralize));
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _tableName = pluralize.Pluralize(typeof(TEntity).Name);

            _table = tableClient.GetTableReference(_tableName);

            var query = _table.CreateQuery<TEntity>();

            _queryable = new ASTQueryableWrapper<TEntity>(query);
        }

        public async Task<TEntity> AddOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return await ExecuteAndLogOperationAsync<TEntity>(TableOperation.InsertOrReplace(entity), cancellationToken).ConfigureAwait(false);
        }

        public Task<long> DeleteAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException($"{nameof(DeleteAsync)} is not supported using {nameof(filter)}, use the deletion by id instead.");
        }

        public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
        {
            var entity = await GetAsync(id, cancellationToken).ConfigureAwait(false);

            await ExecuteAndLogOperationAsync<object>(TableOperation.Delete(entity), cancellationToken).ConfigureAwait(false);

            return true;
        }

        public async Task<TEntity> GetAsync(string id, CancellationToken cancellationToken = default)
        {
            if (id is null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var sections = id.Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries);

            if (sections.Length != 2)
            {
                throw new ArgumentException($"{id} is not valid. The id must be in format {{PartitionKey}}-{{RowKey}}.", nameof(id));
            }

            return await ExecuteAndLogOperationAsync<TEntity>(TableOperation.Retrieve<TEntity>(sections[0], sections[1]), cancellationToken).ConfigureAwait(false);
        }

        public IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return ((IAsyncEnumerable<TEntity>)_queryable).GetAsyncEnumerator(cancellationToken);
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return _queryable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private async Task<TResult> ExecuteAndLogOperationAsync<TResult>(TableOperation operation, CancellationToken cancellationToken)
        {
            TableResult result;
            try
            {
                result = await _table.ExecuteAsync(operation, cancellationToken).ConfigureAwait(false);
            }
            catch (StorageException e)
            {
                if (e.RequestInformation.HttpStatusCode != StatusCodes.Status404NotFound)
                {
                    throw;
                }

                if (await _table.CreateIfNotExistsAsync(cancellationToken).ConfigureAwait(false))
                {
                    _logger.LogTrace($"Table {_tableName} created.");
                }
                else
                {
                    _logger.LogError(e, $"Table {_tableName} already exists.");
                    throw;
                }

                result = await _table.ExecuteAsync(operation, cancellationToken).ConfigureAwait(false);
            }

            if (result.RequestCharge != null)
            {
                _logger.LogTrace($"Request charge for {operation.OperationType} operation ({result.Etag}) for {((TEntity)operation.Entity).Id}: {result.RequestCharge}");
            }

            return (TResult)result.Result;
        }
    }
}

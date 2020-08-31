using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Foralla.KISS.Repository.Wrappers
{
    /// <summary>
    ///     Wrapper for MongoDB to support <see cref="IAsyncEnumerable{T}"/> with the MongoDB
    ///     driver.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the content of the data source.
    /// </typeparam>
    internal class MongoDbQueryableWrapper<T> : IOrderedQueryable<T>, IAsyncEnumerable<T>
    {
        private readonly MongoDbQueryableWrapperProvider _provider;

        public Type ElementType { get; } = typeof(T);

        public Expression Expression { get; }

        public IQueryProvider Provider => _provider;

        public MongoDbQueryableWrapper(IMongoCollection<T> collection)
        {
            var queryable = collection?.AsQueryable() ?? throw new ArgumentNullException(nameof(collection));

            _provider = new MongoDbQueryableWrapperProvider(queryable.Provider);
            Expression = queryable.Expression;
        }
        public MongoDbQueryableWrapper(MongoDbQueryableWrapperProvider provider, Expression expression)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)Provider.Execute(Expression)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            //Since interfaces are internal in the mongo implementation at this level, let's use reflection.
            var executeAsyncMethod = _provider.InternalProvider.GetType().GetMethod("ExecuteAsync").MakeGenericMethod(typeof(IAsyncCursor<T>));

            var cursor = await ((Task<IAsyncCursor<T>>)executeAsyncMethod.Invoke(_provider.InternalProvider, new object[] { Expression, cancellationToken })).ConfigureAwait(false);

            while (await cursor.MoveNextAsync(cancellationToken).ConfigureAwait(false))
            {
                foreach (var item in cursor.Current)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    yield return item;
                }
            }
        }
    }
}

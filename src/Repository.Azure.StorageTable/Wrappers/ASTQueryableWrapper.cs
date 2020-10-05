using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace Foralla.KISS.Repository.Wrappers
{
    internal class ASTQueryableWrapper<T> : IOrderedQueryable<T>, IAsyncEnumerable<T>
    {
        public Type ElementType => typeof(T);
        public Expression Expression { get; }
        public IQueryProvider Provider { get; }

        public ASTQueryableWrapper(TableQuery<T> tableQuery)
        {
            if (tableQuery is null)
            {
                throw new ArgumentNullException(nameof(tableQuery));
            }

            Provider = new ASTQueryableWrapperProvider(tableQuery.Provider);
            Expression = tableQuery.Expression;
        }

        public ASTQueryableWrapper(ASTQueryableWrapperProvider provider, Expression expression)
        {
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            var result = await Task.Run(() => Provider.Execute<TableQuery<T>>(Expression), cancellationToken).ConfigureAwait(false);

            foreach (var item in result)
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return item;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)Provider.Execute(Expression)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

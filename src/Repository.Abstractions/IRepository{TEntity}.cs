using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Foralla.KISS.Repository
{
    public interface IRepository<TEntity, TKey> : IQueryable<TEntity>, IAsyncEnumerable<TEntity>, IRepository<TKey>
        where TKey : struct
        where TEntity : IEntityBase<TKey>
    {
        /// <summary>
        ///     Adds or updates the specified <paramref name="entity"/> to the repository.
        /// </summary>
        /// <param name="entity">The entity to store.</param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> used to control the current operation.
        /// </param>
        /// <returns>
        ///     The entity that is stored when the <see cref="Task{TResult}"/> completes.
        /// </returns>
        Task<TEntity> AddOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Removes the <typeparamref name="TEntity"/> instances that matches the <paramref name="filter"/>.
        /// </summary>
        /// <param name="filter">The expression to use for filtering.</param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> used to control the current operation.
        /// </param>
        /// <returns>
        ///     The number of <typeparamref name="TEntity"/> removed when the <see cref="Task{TResult}"/> completes.
        /// </returns>
        Task<long> DeleteAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Gets the <typeparamref name="TEntity"/> based on the <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id of the <typeparamref name="TEntity"/> to retrieve.</param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> used to control the current operation.
        /// </param>
        /// <returns>
        ///     An instance of <typeparamref name="TEntity"/> if found, otherwise <see langword="null"/> when the 
        ///     <see cref="Task{TResult}"/> completes.
        /// </returns>
        Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default);
    }
}

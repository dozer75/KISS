using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Foralla.KISS.Repository
{
    public abstract class EFRepository<TEntity, TKey> : IRepository<TEntity, TKey>
        where TKey : struct
        where TEntity : class, IEntityBase<TKey>
    {
        private readonly DbContext _context;
        private readonly DbSet<TEntity> _dbSet;
        private readonly IQueryable<TEntity> _queryable;

        public Type ElementType => _queryable.ElementType;
        public Expression Expression => _queryable.Expression;
        public IQueryProvider Provider => _queryable.Provider;

        protected EFRepository(DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<TEntity>();

            _queryable = _dbSet.AsQueryable();
        }

        public async Task<TEntity> AddOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (Equals(entity.Id, default(TKey)))
            {
                await _dbSet.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var changeTracker = _dbSet.Attach(entity);
                changeTracker.State = EntityState.Modified;
            }

            var result = await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);


            return result > 0 ? entity : null;
        }

        public async Task<long> DeleteAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
        {
            var entities = await _dbSet.Where(filter).ToArrayAsync(cancellationToken).ConfigureAwait(false);

            if (entities.Length == 0)
            {
                return 0;
            }

            _dbSet.RemoveRange(entities);


            return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbSet.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
            _context.Entry(entity).State = EntityState.Detached;

            return entity;
        }

        public IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return _dbSet.AsAsyncEnumerable().GetAsyncEnumerator(cancellationToken);
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return _queryable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _queryable.GetEnumerator();
        }

        public async Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken)
        {
            return await DeleteAsync(entity => id.Equals(entity.Id), cancellationToken).ConfigureAwait(false) > 0;
        }
    }
}

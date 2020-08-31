using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Foralla.KISS.Repository.Wrappers;
using MongoDB.Bson;
using MongoDB.Driver;
using Pluralize.NET;

namespace Foralla.KISS.Repository
{
    public abstract class MongoRepository<TEntity> : IRepository<TEntity>
        where TEntity : IEntityBase
    {
        private readonly IMongoCollection<TEntity> _collection;
        private readonly IClientSessionHandle _session;
        private readonly IQueryable<TEntity> _queryable;

        public Type ElementType => _queryable.ElementType;
        public Expression Expression => _queryable.Expression;
        public IQueryProvider Provider => _queryable.Provider;

        protected MongoRepository(IClientSessionHandle session, IMongoDatabase database, IMongoModelBuilder<TEntity> builder, IPluralize pluralize)
        {
            if (database == null)
            {
                throw new ArgumentNullException(nameof(database));
            }

            if (pluralize == null)
            {
                throw new ArgumentNullException(nameof(pluralize));
            }

            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            _session = session ?? throw new ArgumentNullException(nameof(session));

            builder.CreateModel();

            var collectionName = pluralize.Pluralize(typeof(TEntity).Name);

            lock (database)
            {
                if (!database.ListCollectionNames(new ListCollectionNamesOptions
                {
                    Filter = new BsonDocument("name", collectionName)
                })
                             .Any())
                {
                    database.CreateCollection(collectionName);
                }
            }

            _collection = database.GetCollection<TEntity>(collectionName);

            builder.CreateIndex(_collection.Indexes);

            _queryable = new MongoDbQueryableWrapper<TEntity>(_collection);
        }

        public async Task<TEntity> AddOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (entity.Id == default)
            {
                await _collection.InsertOneAsync(_session, entity, new InsertOneOptions { BypassDocumentValidation = false }, cancellationToken)
                                 .ConfigureAwait(false);

                return entity;
            }

            return await _collection.FindOneAndReplaceAsync(_session, e => e.Id == entity.Id, entity,
                                                            new FindOneAndReplaceOptions<TEntity, TEntity>
                                                            {
                                                                ReturnDocument = ReturnDocument.After,
                                                                BypassDocumentValidation = false
                                                            },
                                                            cancellationToken)
                                    .ConfigureAwait(false);
        }

        public async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await _collection.InsertOneAsync(_session, entity, new InsertOneOptions { BypassDocumentValidation = false }, cancellationToken)
                             .ConfigureAwait(false);

            return entity;
        }

        public async Task<long> DeleteAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
        {
            var result = await _collection.DeleteManyAsync(_session, filter, cancellationToken: cancellationToken).ConfigureAwait(false);

            return result.DeletedCount;
        }

        public async Task<TEntity> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var findResult = await _collection.FindAsync(entity => entity.Id == id, cancellationToken: cancellationToken)
                                              .ConfigureAwait(false);

            return await findResult.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
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

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            return await DeleteAsync(entity => entity.Id == id, cancellationToken).ConfigureAwait(false) > 0;
        }
    }
}

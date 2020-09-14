using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Foralla.KISS.Repository
{
    public abstract class MongoModelBuilder<TEntity, TKey> : IMongoModelBuilder<TEntity, TKey>
        where TKey : struct
        where TEntity : IEntityBase<TKey>
    {
        public void CreateModel()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(TEntity)))
            {
                BsonClassMap.RegisterClassMap<TEntity>(CreateModel);
            }
        }

        public abstract void CreateIndex(IMongoIndexManager<TEntity> indexManager);

        public abstract void CreateModel(BsonClassMap<TEntity> builder);
    }
}

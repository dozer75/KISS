using MongoDB.Driver;

namespace Foralla.KISS.Repository
{
    public interface IMongoModelBuilder<TEntity, TKey>
        where TKey : struct
        where TEntity : IEntityBase<TKey>
    {
        void CreateIndex(IMongoIndexManager<TEntity> indexManager);

        void CreateModel();
    }
}

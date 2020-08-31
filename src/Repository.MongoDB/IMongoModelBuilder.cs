using MongoDB.Driver;

namespace Foralla.KISS.Repository
{
    public interface IMongoModelBuilder<TEntity>
        where TEntity : IEntityBase
    {
        void CreateIndex(IMongoIndexManager<TEntity> indexManager);

        void CreateModel();
    }
}

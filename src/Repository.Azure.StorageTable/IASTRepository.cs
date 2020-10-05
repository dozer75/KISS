namespace Foralla.KISS.Repository
{
    public interface IASTRepository<TEntity> : IRepository<TEntity, string>
        where TEntity : ASTEntityBase, new()
    {
    }
}

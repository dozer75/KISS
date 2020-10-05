namespace Foralla.KISS.Repository
{
    public interface IEntityBase<TKey>
    {
        TKey Id { get; set; }
    }
}

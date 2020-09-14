namespace Foralla.KISS.Repository
{
    public interface IEntityBase<TKey>
        where TKey : struct
    {
        TKey Id { get; set; }
    }
}

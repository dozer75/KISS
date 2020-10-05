using System.Threading;
using System.Threading.Tasks;

namespace Foralla.KISS.Repository
{
    public interface IRepository<in TKey>
    {
        /// <summary>
        ///     Removes the entity that matches <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id of the entity to remove.</param>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken"/> used to control the current operation.
        /// </param>
        /// <returns>
        ///     True if the entity was removed, otherwise false.
        /// </returns>
        Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken);
    }
}

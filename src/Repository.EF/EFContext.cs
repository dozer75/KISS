using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Foralla.KISS.Repository
{
    /// <summary>
    ///     Default <see cref="DbContext"/> implementation for the framework.
    /// </summary>
    internal sealed class EFContext : DbContext
    {
        private readonly EFOptions _options;
        private readonly IEnumerable<IEFModelBuilder> _entityBuilders;

        public EFContext(EFOptions options, IEnumerable<IEFModelBuilder> entityBuilders)
        {
            _options = options;
            _entityBuilders = entityBuilders;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            _options.OnConfiguring.Invoke(builder);
            base.OnConfiguring(builder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            foreach (var entityBuilder in _entityBuilders)
            {
                entityBuilder.CreateModel(builder);
            }

            _options.OnModelCreating?.Invoke(builder);

            base.OnModelCreating(builder);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnSavingChanges();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            OnSavingChanges();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void OnSavingChanges()
        {
            _options.OnSavingChanges?.Invoke(ChangeTracker.Entries()
                                                           .Where(entry => entry.State != EntityState.Unchanged && entry.State != EntityState.Detached)
                                                           .ToArray());
        }
    }
}

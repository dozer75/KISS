using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Foralla.KISS.Repository
{
    internal class EFContext : DbContext
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
            _options.OnConfiguring?.Invoke(builder);
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
    }
}

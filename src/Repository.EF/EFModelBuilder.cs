using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foralla.KISS.Repository
{
    public abstract class EFModelBuilder<TEntity> : IEFModelBuilder
        where TEntity : class, IEntityBase
    {
        public void CreateModel(ModelBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Entity<TEntity>(CreateModel);
        }

        public abstract void CreateModel(EntityTypeBuilder<TEntity> builder);
    }
}

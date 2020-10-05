using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Foralla.KISS.Repository
{
    public abstract class EFModelBuilder<TEntity, TKey> : IEFModelBuilder
        where TKey : struct
        where TEntity : class, IEntityBase<TKey>
    {
        public void CreateModel(ModelBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Entity<TEntity>(CreateModel);
        }

        public abstract void CreateModel(EntityTypeBuilder<TEntity> builder);
    }
}

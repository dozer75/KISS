using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Foralla.KISS.Repository
{
    /// <summary>
    ///     Configuration class used by 
    ///     <see cref="Extensions.EFExtensions.AddEFRepository"/> to configure the underlying <see cref="DbContext"/>.
    /// </summary>
    public class EFOptions
    {
        /// <summary>
        ///     Gets or sets an action that is called when the underlying <see cref="DbContext"/> is being configured.
        /// </summary>
        /// <remarks>
        ///     Use this to configure the context used by the framework.
        /// </remarks>
        public Action<DbContextOptionsBuilder> OnConfiguring { get; set; }

        /// <summary>
        ///     Gets or sets an action that is called when the model is about to be created.
        /// </summary>
        /// <remarks>
        ///     This is called after all <see cref="IEFModelBuilder"/> implementations.
        /// </remarks>
        public Action<ModelBuilder> OnModelCreating { get; set; }

        /// <summary>
        ///     Gets or sets an action that is called whenever changes is about to be persisted.
        /// </summary>
        /// <remarks>
        ///     The action retrieves all <see cref="EntityEntry"/> that is currently in either
        ///     <see cref="EntityState.Added"/>, <see cref="EntityState.Deleted"/> or
        ///     <see cref="EntityState.Modified"/>.
        /// </remarks>
        public Action<EntityEntry[]> OnSavingChanges { get; set; }
    }
}

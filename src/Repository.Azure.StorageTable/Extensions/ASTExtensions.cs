using System;
using System.Reflection;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pluralize.NET;

namespace Foralla.KISS.Repository.Extensions
{
    public static class ASTExtensions
    {
        /// <summary>
        ///     Configures the <paramref name="services"/> for Azure Storage Table support.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to apply Azure Storage Table support.</param>
        /// <param name="configure">
        ///     A configuration action that supplies a <see cref="ASTOptions"/> that can be used for configurations.
        /// 
        ///     A <see cref="IServiceProvider"/> is also supplied to be able to the resulting <paramref name="services"/>
        ///     to retrieve configuration during initialization.
        /// </param>
        /// <param name="scanAssemblies">
        ///     Assemblies to scan for <see cref="IASTRepository{TEntity}"/> repositories.
        /// </param>
        /// <returns>The updated <paramref name="services"/> instance.</returns>
        public static IServiceCollection AddAzureStorageTableRepository(this IServiceCollection services, Action<ASTOptions, IServiceProvider> configure,
                                                                        params Assembly[] scanAssemblies)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configure is null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            services.TryAddSingleton<IPluralize, Pluralizer>();

            services.TryAddSingleton(p =>
                                     {
                                         var options = new ASTOptions();

                                         configure(options, p);

                                         if (string.IsNullOrWhiteSpace(options.ConnectionString))
                                         {
                                             throw new ArgumentException($"{nameof(ASTOptions)}.{nameof(options.ConnectionString)} is null.", nameof(configure));
                                         }

                                         return options;
                                     });

            services.TryAddSingleton(p =>
                                     {
                                         var options = p.GetRequiredService<ASTOptions>();

                                         return CloudStorageAccount.Parse(options.ConnectionString);
                                     });

            services.TryAddScoped(p =>
                                  {
                                      var options = p.GetRequiredService<ASTOptions>();
                                      var csAccount = p.GetRequiredService<CloudStorageAccount>();

                                      return csAccount.CreateCloudTableClient(options.CosmosClientConfiguration);
                                  });

            services.AddRepositories(scanAssemblies);

            return services;
        }
    }
}

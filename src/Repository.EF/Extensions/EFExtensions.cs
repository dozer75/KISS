using System;
using System.Linq;
using System.Reflection;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pluralize.NET;

namespace Foralla.KISS.Repository.Extensions
{
    public static class EFExtensions
    {
        /// <summary>
        ///     Configures the <paramref name="services"/> for Entity Framework support.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to apply Entity Framework support.</param>
        /// <param name="configure">
        ///     A configuration action that supplies a <see cref="EFOptions"/> that can be used for configurations.
        /// 
        ///     A <see cref="IServiceProvider"/> is also supplied to be able to the resulting <paramref name="services"/>
        ///     to retrieve configuration during initialization.
        /// </param>
        /// <param name="scanAssemblies">
        ///     Assemblies to scan for <see cref="IEntityBase{TKey}"/> and <see cref="IEFModelBuilder"/>
        ///     instances to configure the Entity Framework support.
        /// </param>
        /// <returns>The updated <paramref name="services"/> instance.</returns>
        public static IServiceCollection AddEFRepository(this IServiceCollection services, Action<EFOptions, IServiceProvider> configure,
                                                         params Assembly[] scanAssemblies)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            services.TryAddSingleton<IPluralize, Pluralizer>();

            services.TryAddSingleton(p =>
            {
                var options = new EFOptions();

                configure(options, p);

                if (options.OnConfiguring == null)
                {
                    throw new ArgumentException($"{nameof(EFOptions)}.{nameof(options.OnConfiguring)} is null.", nameof(configure));
                }

                return options;
            });

            services.TryAddTransient<DbContext, EFContext>();

            services.TryAddScoped(p => new TransactionScope(TransactionScopeOption.RequiresNew,
                                                            new TransactionOptions
                                                            {
                                                                IsolationLevel = IsolationLevel.ReadCommitted
                                                            },
                                                            TransactionScopeAsyncFlowOption.Enabled));

            services.AddScoped<ITransaction, EFTransaction>();

            services.AddRepositories(scanAssemblies);

            var builders = (scanAssemblies.Any() ? scanAssemblies : Assembly.GetCallingAssembly().GetReferencedAssemblies().Select(Assembly.Load)
                                                                            .Union(new[] { Assembly.GetCallingAssembly() }))
                    .SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Any(it => it == typeof(IEFModelBuilder))))
                    .ToArray();

            foreach (var builder in builders)
            {
                if (!services.Any(sd => sd.ServiceType == typeof(IEFModelBuilder) && sd.ImplementationType == builder))
                {
                    services.AddSingleton(typeof(IEFModelBuilder), builder);
                }
            }

            return services;
        }
    }
}

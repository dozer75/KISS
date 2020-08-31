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

            services.AddSingleton(p =>
            {
                var options = new EFOptions();

                configure(options, p);

                if (options.OnConfiguring == null)
                {
                    throw new ArgumentException($"{nameof(EFOptions)}.{options.OnConfiguring} must be set.", nameof(configure));
                }

                return options;
            });

            services.AddTransient<DbContext, EFContext>();

            services.TryAddScoped(p => new TransactionScope(TransactionScopeOption.RequiresNew,
                                                            new TransactionOptions
                                                            {
                                                                IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted
                                                            },
                                                            TransactionScopeAsyncFlowOption.Enabled));

            services.AddScoped<ITransaction, EFTransaction>();

            var builders = (scanAssemblies.Any() ?
                scanAssemblies :
                Assembly.GetEntryAssembly()?.GetReferencedAssemblies().Select(Assembly.Load)
                    .Union(new[] { Assembly.GetEntryAssembly() }))
                    .SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Any(it => it == typeof(IEFModelBuilder))))
                    .ToArray();

            foreach (var builder in builders)
            {
                services.AddSingleton(typeof(IEFModelBuilder), builder);
            }

            return services;
        }
    }
}

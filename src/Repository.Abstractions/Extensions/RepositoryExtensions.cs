using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Foralla.KISS.Repository.Extensions
{
    public static class RepositoryExtensions
    {
        /// <summary>
        ///     Adds all <see cref="IRepository{TKey}"/> implementations to the specified <paramref name="services"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to apply Entity Framework support.</param>
        /// <param name="scanAssemblies">
        ///     Assemblies to scan for <see cref="IEntityBase{TKey}"/> and <see cref="IEFModelBuilder"/>
        ///     instances to configure the Entity Framework support.
        /// </param>
        /// <returns>The updated <paramref name="services"/> instance.</returns>
        public static IServiceCollection AddRepositories(this IServiceCollection services, params Assembly[] scanAssemblies)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var repositories = (scanAssemblies.Any() ? scanAssemblies : Assembly.GetCallingAssembly().GetReferencedAssemblies().Select(Assembly.Load)
                                                                                .Union(new[] { Assembly.GetCallingAssembly() }))
                        .SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Any(IsRepository)))
                        .ToArray();

            foreach (var repository in repositories)
            {
                var allInterfaces = repository.GetInterfaces();

                foreach (var it in repository.GetInterfaces().Except(allInterfaces.SelectMany(ai => ai.GetInterfaces()))
                                             .Where(it => it != typeof(IDisposable)))
                {
                    services.TryAddScoped(it, repository);
                }
            }

            return services;
        }

        private static bool IsRepository(Type it)
        {
            return it.IsGenericType && it.GetGenericTypeDefinition() == typeof(IRepository<>);
        }
    }
}

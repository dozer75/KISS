using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Foralla.KISS.Repository.Extensions
{
    public static class RepositoryExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services,
                                                         params Assembly[] scanAssemblies)
        {

            var repositories = (scanAssemblies.Any() ?
                scanAssemblies :
                Assembly.GetEntryAssembly()?.GetReferencedAssemblies().Select(Assembly.Load)
                    .Union(new[] { Assembly.GetEntryAssembly() }))
                    .SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Any(IsRepository)))
                    .ToArray();

            foreach (var repository in repositories)
            {
                foreach (var it in repository.GetInterfaces()
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

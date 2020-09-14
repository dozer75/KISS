﻿using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;
using Pluralize.NET;

namespace Foralla.KISS.Repository.Extensions
{
    public static class MongoExtensions
    {
        public static IServiceCollection AddMongoRepository(this IServiceCollection services,
                                                            Action<MongoOptions, IServiceProvider> configure,
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
                var options = new MongoOptions();

                configure(options, p);

                if (options.ServerSettings == null)
                {
                    throw new ArgumentException($"{nameof(MongoOptions)}.{options.ServerSettings} must be set.", nameof(configure));
                }

                if (options.DatabaseName == null)
                {
                    throw new ArgumentException($"{nameof(MongoOptions)}.{options.DatabaseName} must be set.", nameof(configure));
                }

                return options;
            });

            services.TryAddSingleton<IMongoClient>(p =>
            {
                var options = new MongoOptions();

                configure(options, p);

                return new MongoClient(options.ServerSettings);
            });

            services.TryAddSingleton(p =>
            {
                var client = p.GetRequiredService<IMongoClient>();
                var options = p.GetRequiredService<MongoOptions>();

                return client.GetDatabase(options.DatabaseName, options.DatabaseSettings);
            });

            services.TryAddScoped(p =>
            {
                var db = p.GetRequiredService<IMongoClient>();
                var options = p.GetRequiredService<MongoOptions>();

                return db.StartSession(options.ClientSessionOptions);
            });

            services.AddScoped<ITransaction, MongoTransaction>();

            var builders = (scanAssemblies.Any() ?
                scanAssemblies :
                Assembly.GetEntryAssembly()?.GetReferencedAssemblies().Select(Assembly.Load)
                    .Union(new[] { Assembly.GetEntryAssembly() }))
                    .SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract &&
                                                             t.GetInterfaces().Any(it => it.IsGenericType &&
                                                                                         it.GetGenericTypeDefinition() == typeof(IMongoModelBuilder<,>))))
                    .ToArray();

            foreach (var builder in builders)
            {
                services.TryAddSingleton(builder.GetInterfaces().First(it => it.IsGenericType && it.GetGenericTypeDefinition() == typeof(IMongoModelBuilder<,>)), builder);
            }

            return services;
        }
    }
}

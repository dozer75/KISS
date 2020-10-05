﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Foralla.KISS.Repository.Wrappers
{
    internal class ASTQueryableWrapperProvider : IQueryProvider
    {
        private readonly IQueryProvider _queryProvider;

        public ASTQueryableWrapperProvider(IQueryProvider queryProvider)
        {
            if (queryProvider is null)
            {
                throw new ArgumentNullException(nameof(queryProvider));
            }

            _queryProvider = queryProvider;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = GetSequenceElementType(expression.Type);

            var query = _queryProvider.CreateQuery(expression);

            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(ASTQueryableWrapper<>).MakeGenericType(elementType), this, query.Expression);
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException ?? e;
            }
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            var query = _queryProvider.CreateQuery<TElement>(expression);

            return new ASTQueryableWrapper<TElement>(this, query.Expression);
        }


        public object Execute(Expression expression)
        {
            return _queryProvider.CreateQuery(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult)Execute(expression);
        }

        private static Type GetSequenceElementType(Type type)
        {
            var result = FindEnumerable(type);

            return result is null ? type : result.GetTypeInfo().GetGenericArguments()[0];
        }

        private static Type FindEnumerable(Type type)
        {
            if (type is null || type == typeof(string))
            {
                return null;
            }

            var typeInfo = type.GetTypeInfo();

            if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return type;
            }

            if (typeInfo.IsArray)
            {
                return typeof(IEnumerable<>).MakeGenericType(type.GetElementType());
            }

            if (typeInfo.IsGenericType)
            {
                foreach (var arg in type.GetTypeInfo().GetGenericArguments())
                {
                    var enumerable = typeof(IEnumerable<>).MakeGenericType(arg);
                    if (enumerable.GetTypeInfo().IsAssignableFrom(type))
                    {
                        return enumerable;
                    }
                }
            }

            foreach (var iface in typeInfo.GetInterfaces())
            {
                var enumerable = FindEnumerable(iface);

                if (enumerable != null)
                {
                    return enumerable;
                }
            }

            return typeInfo.BaseType != null && typeInfo.BaseType != typeof(object) ?
                       FindEnumerable(typeInfo.BaseType) :
                       null;
        }
    }
}
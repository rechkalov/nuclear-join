using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NuClear.Utils.Join
{
    internal sealed class MemoryJoinQueryable<T> : IQueryable<T>, ISecretMarker
    {
        public MemoryJoinQueryable(IQueryProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            Provider = provider;
            Expression = Expression.Constant(this, typeof(IQueryable<T>));
        }

        public MemoryJoinQueryable(IQueryProvider provider, Expression expression)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (!typeof(IQueryable<T>).GetTypeInfo().IsAssignableFrom(expression.Type.GetTypeInfo()))
            {
                throw new ArgumentOutOfRangeException(nameof(expression));
            }

            Provider = provider;
            Expression = expression;
        }

        public Expression Expression { get; }
        public IQueryProvider Provider { get; }
        public Type ElementType { get; } = typeof(T);

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)Provider.Execute(Expression)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
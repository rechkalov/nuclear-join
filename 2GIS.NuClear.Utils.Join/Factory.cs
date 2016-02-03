using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NuClear.Utils.Join
{
    public class Factory
    {
        public static IQueryable<TResult> Create<T1, T2, TKey, TResult>(
            IQueryable<T1> left,
            Expression<Func<T1, TKey>> leftKeyExpression,
            IQueryable<T2> right,
            Expression<Func<T2, TKey>> rightKeyExpression,
            Expression<Func<T1, T2, TResult>> joinExpression)
            where TKey : IComparable
        {
            var iterationStrategy = new SimpleJoinStrategy<T1, T2, TKey>(left, leftKeyExpression, right, rightKeyExpression, Comparer<TKey>.Default);
            var provider = new MemoryJoinProvider<T1, T2, TResult>(iterationStrategy, joinExpression);
            return new MemoryJoinQueryable<TResult>(provider);
        }
    }
}

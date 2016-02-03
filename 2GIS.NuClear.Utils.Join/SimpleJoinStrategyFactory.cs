using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NuClear.Utils.Join
{
    public sealed class SimpleJoinStrategyFactory : IJoinStrategyFactory
    {
        public IJoinStrategy<T1, T2> Create<T1, T2, TKey>(IQueryable<T1> left, Expression<Func<T1, TKey>> leftKeyExpression, IQueryable<T2> right,
            Expression<Func<T2, TKey>> rightKeyExpression, IComparer<TKey> keyComparer)
        {
            return new SimpleJoinStrategy<T1,T2,TKey>(left, leftKeyExpression, right, rightKeyExpression, keyComparer);
        }
    }
}
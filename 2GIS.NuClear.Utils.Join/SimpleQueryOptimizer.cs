using System;
using System.Linq;
using System.Linq.Expressions;

namespace NuClear.Utils.Join
{
    public sealed class SimpleQueryOptimizer<T1, T2, TKey> : IQueryOptimizer<T1, T2>
    {
        private readonly Expression<Func<T1, TKey>> _leftKeySelector;
        private readonly Expression<Func<T2, TKey>> _rightKeySelector;

        public SimpleQueryOptimizer(Expression<Func<T1, TKey>> leftKeySelector, Expression<Func<T2, TKey>> rightKeySelector)
        {
            _leftKeySelector = leftKeySelector;
            _rightKeySelector = rightKeySelector;
        }

        public bool TryOptimize(IQueryable<T1> left, IQueryable<T2> right, out IQueryable<T1> leftOptimized, out IQueryable<T2> rightOptimized)
        {
            leftOptimized = left.OrderBy(_leftKeySelector);
            rightOptimized = right.OrderBy(_rightKeySelector);
            return true;
        }
    }
}